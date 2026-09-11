using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Buildalyzer;
using NuGet.Frameworks;
using NuGet.ProjectModel;
using Snitch.Analysis.Utilities;
using Spectre.Console;

namespace Snitch.Analysis
{
    internal sealed class ProjectBuilder
    {
        private readonly IAnsiConsole _console;

        public ProjectBuilder(IAnsiConsole console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public ProjectBuildResult Build(
            string path,
            string? tfm,
            string[]? skip,
            IEnumerable<Project>? cache = null)
        {
            var manager = new AnalyzerManager();

            // Keyed by the full path — two projects can share a file name — plus the
            // requested target framework, since a multi targeting project referenced
            // from two roots yields different packages for each of them.
            var built = new Dictionary<string, Project>(StringComparer.OrdinalIgnoreCase);
            foreach (var cached in cache ?? Enumerable.Empty<Project>())
            {
                Remember(built, cached);
            }

            var project = Build(manager, path, tfm, skip, built);

            // Get all dependencies which are all built projects minus the project.
            var dependencies = new HashSet<Project>(built.Values, new ProjectComparer());
            dependencies.Remove(project);

            return new ProjectBuildResult(project, dependencies);
        }

        private Project Build(
            AnalyzerManager manager,
            string path,
            string? tfm,
            string[]? skip,
            Dictionary<string, Project> built,
            int indentation = 0)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            if (built == null)
            {
                throw new ArgumentNullException(nameof(built));
            }

            path = Path.GetFullPath(path);

            // Already built this project for this target framework?
            if (built.TryGetValue(GetCacheKey(path, tfm), out var project))
            {
                return project;
            }

            project = new Project(path, tfm);

            var result = Build(manager, project, tfm, indentation);
            if (result == null)
            {
                throw new InvalidOperationException($"Could not build {path}.");
            }

            // Get the asset path.
            var assetPath = result.GetProjectAssetsFilePath();
            if (!string.IsNullOrWhiteSpace(assetPath))
            {
                if (!File.Exists(assetPath))
                {
                    // Todo: Make sure this exists in future
                    throw new InvalidOperationException($"{assetPath} not found. Please restore the project's dependencies before running Snitch.");
                }
            }
            else
            {
                var prefix = new string(' ', indentation * 2);
                if (indentation > 0)
                {
                    prefix += "  ";
                }

                _console.MarkupLine($"{prefix}[yellow]WARN:[/] Old CSPROJ format can't be analyzed");
            }

            // Set project information.
            project.TargetFramework = result.TargetFramework;
            project.LockFilePath = assetPath;

            // Add the project to the built list.
            Remember(built, project);

            // Get the package references.
            var restoredVersions = GetRestoredVersions(assetPath, project.TargetFramework);
            foreach (var packageReference in result.PackageReferences)
            {
                var version = packageReference.Value.GetValueOrDefault("Version");
                var privateAssets = packageReference.Value.GetValueOrDefault("PrivateAssets");

                if (string.IsNullOrWhiteSpace(version))
                {
                    // Central Package Management keeps the version out of the reference,
                    // so fall back to the version that restore actually settled on.
                    version = restoredVersions.GetValueOrDefault(packageReference.Key);
                }

                project.Packages.Add(new Package(packageReference.Key, version, privateAssets));
            }

            // Analyze all project references.
            foreach (var projectReference in result.ProjectReferences)
            {
                var projectReferencePath = PathUtility.GetPathRelativeToProject(project, projectReference);

                if (skip != null)
                {
                    var projectName = Path.GetFileNameWithoutExtension(projectReferencePath);
                    if (skip.Contains(projectName, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                if (!projectReferencePath.EndsWith("csproj", StringComparison.OrdinalIgnoreCase) && !projectReferencePath.EndsWith("fsproj", StringComparison.OrdinalIgnoreCase))
                {
                    _console.MarkupLine(string.IsNullOrWhiteSpace(tfm)
                        ? $"Skipping Non .NET Project [aqua]{project.Name.EscapeMarkup()}[/]"
                        : $"Skipping Non .NET Project [aqua]{project.Name.EscapeMarkup()}[/] [grey] ({tfm.EscapeMarkup()})[/]");

                    _console.WriteLine();

                    continue;
                }

                var analyzedProjectReference = Build(manager, projectReferencePath, project.TargetFramework, skip, built, indentation + 1);
                project.ProjectReferences.Add(analyzedProjectReference);
            }

            return project;
        }

        private static void Remember(Dictionary<string, Project> built, Project project)
        {
            // Under the framework that was asked for, and under the one it resolved to.
            // Without the latter a project built with no framework in particular would be
            // built again the moment a parent asks for the very framework it settled on.
            built[GetCacheKey(project)] = project;
            built[GetCacheKey(project.Path, project.TargetFramework)] = project;
        }

        private static string GetCacheKey(Project project)
        {
            return GetCacheKey(project.Path, project.RequestedTargetFramework);
        }

        private static string GetCacheKey(string path, string? tfm)
        {
            return $"{path}|{tfm}";
        }

        private static Dictionary<string, string> GetRestoredVersions(string? lockFilePath, string? targetFramework)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(lockFilePath) || string.IsNullOrWhiteSpace(targetFramework))
            {
                return result;
            }

            var lockFile = new LockFileFormat().Read(lockFilePath);
            var framework = NuGetFramework.Parse(targetFramework);

            // Versions can be conditioned per target framework, so the exact one has to win
            // over another one that merely shares the same framework identifier.
            var target = lockFile.PackageSpec.TargetFrameworks.FirstOrDefault(
                    x => x.FrameworkName.Equals(framework))
                ?? lockFile.PackageSpec.TargetFrameworks.FirstOrDefault(
                    x => x.FrameworkName.Framework.Equals(framework.Framework, StringComparison.OrdinalIgnoreCase));

            if (target == null)
            {
                return result;
            }

            foreach (var dependency in target.Dependencies)
            {
                var version = dependency.LibraryRange?.VersionRange?.MinVersion;
                if (version != null)
                {
                    result[dependency.Name] = version.ToNormalizedString();
                }
            }

            return result;
        }

        private IAnalyzerResult? Build(AnalyzerManager manager, Project project, string? tfm, int indentation)
        {
            var prefix = new string(' ', indentation * 2);
            if (indentation > 0)
            {
                prefix += "[grey]*[/] ";
            }

            var status = string.IsNullOrWhiteSpace(tfm)
                ? $"{prefix}Analyzing [aqua]{project.Name.EscapeMarkup()}[/]..."
                : $"{prefix}Analyzing [aqua]{project.Name.EscapeMarkup()}[/] [grey]({tfm.EscapeMarkup()})[/]...";

            _console.MarkupLine(status);

            var projectAnalyzer = manager.GetProject(project.Path);
            var results = (IEnumerable<IAnalyzerResult>)projectAnalyzer.Build();

            if (!string.IsNullOrWhiteSpace(tfm))
            {
                var closest = results.GetNearestFrameworkMoniker(tfm);
                results = results.Where(p => closest.Equals(p.TargetFramework, StringComparison.OrdinalIgnoreCase));
            }

            return results.FirstOrDefault();
        }
    }
}