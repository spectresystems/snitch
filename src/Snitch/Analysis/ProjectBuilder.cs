using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Buildalyzer;

using Snitch.Analysis.Utilities;

namespace Snitch.Analysis
{
    internal sealed class ProjectBuilder
    {
        private readonly IConsole _console;

        public ProjectBuilder(IConsole console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public ProjectBuildResult Build(string path, string? tfm, string[]? skip, IEnumerable<Project>? cache = null)
        {
            var manager = new AnalyzerManager();
            var built = cache?.ToDictionary(x => x.File, x => x, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, Project>(StringComparer.OrdinalIgnoreCase);

            // Build the specified project.
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
            Dictionary<string, Project> built)
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

            var result = Build(manager, project, tfm);
            if (result == null)
            {
                throw new InvalidOperationException($"Could not build {path}.");
            }

            // Get the asset path.
            var assetPath = result.GetProjectAssetsFilePath();
            if (!File.Exists(assetPath))
            {
                // Todo: Make sure this exists in future
                throw new InvalidOperationException($"{assetPath} not found. Please run 'dotnet restore'.");
            }

            // Set project information.
            project.TargetFramework = result.TargetFramework;
            project.LockFilePath = result.GetProjectAssetsFilePath();

            // Add the project to the built list.
            built.Add(Path.GetFileName(path), project);

            // Get the package references.
            foreach (var packageReference in result.PackageReferences)
            {
                if (packageReference.Value.TryGetValue("Version", out var version))
                {
                    project.Packages.Add(new Package(packageReference.Key, version));
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

                if (!projectReferencePath.EndsWith("csproj", StringComparison.OrdinalIgnoreCase))
                {
                    _console.Write("   -> Skipping Non C# Project ");
                    _console.ForegroundColor = ConsoleColor.Cyan;
                    _console.Write(project.Name);
                    _console.ResetColor();
                    if (!string.IsNullOrWhiteSpace(tfm))
                    {
                        _console.ForegroundColor = ConsoleColor.DarkGray;
                        _console.Write(" (");
                        _console.Write(tfm);
                        _console.Write(")");
                        _console.ResetColor();
                    }

                    _console.WriteLine();

                    continue;
                }

                var analyzedProjectReference = Build(manager, projectReferencePath, project.TargetFramework, skip, built);
                project.ProjectReferences.Add(analyzedProjectReference);
            }

            return project;
        }

        private AnalyzerResult Build(AnalyzerManager manager, Project project, string? tfm)
        {
            _console.Write("   -> Analyzing ");
            _console.ForegroundColor = ConsoleColor.Cyan;
            _console.Write(project.Name);
            _console.ResetColor();

            if (!string.IsNullOrWhiteSpace(tfm))
            {
                _console.ForegroundColor = ConsoleColor.DarkGray;
                _console.Write(" (");
                _console.Write(tfm);
                _console.Write(")");
                _console.ResetColor();
                _console.WriteLine();
            }

            var projectAnalyzer = manager.GetProject(project.Path);
            var results = (IEnumerable<AnalyzerResult>)projectAnalyzer.Build();

            if (!string.IsNullOrWhiteSpace(tfm))
            {
                var closest = results.GetNearestFrameworkMoniker(tfm);
                results = results.Where(p => p.TargetFramework.Equals(closest, StringComparison.OrdinalIgnoreCase));
            }

            var result = results.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(tfm))
            {
                _console.ForegroundColor = ConsoleColor.DarkGray;
                _console.Write(" (");
                _console.Write(result.TargetFramework);
                _console.ForegroundColor = ConsoleColor.DarkGray;
                _console.WriteLine(")");
                _console.ResetColor();
            }

            return result;
        }
    }
}
