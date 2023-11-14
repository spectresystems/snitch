using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Buildalyzer;
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
            var built = cache?.ToDictionary(x => x.File, x => x, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, Project>(StringComparer.OrdinalIgnoreCase);

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

            // Already built this project?
            if (built.TryGetValue(Path.GetFileName(path), out var project))
            {
                return project;
            }

            project = new Project(path);

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
            built.Add(Path.GetFileName(path), project);

            // Get the package references.
            foreach (var packageReference in result.PackageReferences)
            {
                if (packageReference.Value.TryGetValue("Version", out var version))
                {
                    var privateAssets = packageReference.Value.GetValueOrDefault("PrivateAssets");

                    project.Packages.Add(new Package(packageReference.Key, version, privateAssets));
                }
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
                        ? $"Skipping Non .NET Project [aqua]{project.Name}[/]"
                        : $"Skipping Non .NET Project [aqua]{project.Name}[/] [grey] ({tfm})[/]");

                    _console.WriteLine();

                    continue;
                }

                var analyzedProjectReference = Build(manager, projectReferencePath, project.TargetFramework, skip, built, indentation + 1);
                project.ProjectReferences.Add(analyzedProjectReference);
            }

            return project;
        }

        private IAnalyzerResult? Build(AnalyzerManager manager, Project project, string? tfm, int indentation)
        {
            var prefix = new string(' ', indentation * 2);
            if (indentation > 0)
            {
                prefix += "[grey]*[/] ";
            }

            var status = string.IsNullOrWhiteSpace(tfm)
                ? $"{prefix}Analyzing [aqua]{project.Name}[/]..."
                : $"{prefix}Analyzing [aqua]{project.Name}[/] [grey]({tfm})[/]...";

            _console.MarkupLine(status);

            var projectAnalyzer = manager.GetProject(project.Path);
            var results = (IEnumerable<IAnalyzerResult>)projectAnalyzer.Build();

            if (!string.IsNullOrWhiteSpace(tfm))
            {
                var closest = results.GetNearestFrameworkMoniker(tfm);
                results = results.Where(p => p.TargetFramework.Equals(closest, StringComparison.OrdinalIgnoreCase));
            }

            return results.FirstOrDefault();
        }
    }
}