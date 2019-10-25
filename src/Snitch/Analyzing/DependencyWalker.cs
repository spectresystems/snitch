using Buildalyzer;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Snitch.Analyzing
{
    public static class DependencyWalker
    {
        public static Project Collect(string path, string tfm)
        {
            var manager = new AnalyzerManager();
            var built = new Dictionary<string, Project>(StringComparer.OrdinalIgnoreCase);
            return Analyze(manager, path, tfm, built);
        }

        public static Project Analyze(AnalyzerManager manager, string path, string tfm, Dictionary<string, Project> built)
        {
            if (built.TryGetValue(Path.GetFileName(path), out var project))
            {
                return project;
            }

            project = new Project(path);

            var buildResult = Build(manager, project, tfm);
            if (buildResult == null)
            {
                throw new InvalidOperationException($"Could not build {path}");
            }

            // Set the target framework.
            project.TargetFramework = buildResult.TargetFramework;

            // Add the project to the built list.
            built.Add(Path.GetFileName(path), project);

            // Get the package references.
            foreach (var packageReference in buildResult.PackageReferences)
            {
                if (packageReference.Value.TryGetValue("Version", out var version))
                {
                    project.Packages.Add(new Package
                    {
                        Name = packageReference.Key,
                        Version = version
                    });
                }
            }

            // Analyze all projects.
            foreach (var projectReference in buildResult.ProjectReferences)
            {
                var projectReferencePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), projectReference));
                var analyzedProjectReference = Analyze(manager, projectReferencePath, project.TargetFramework, built);

                project.ProjectReferences.Add(analyzedProjectReference);
            }

            return project;
        }

        private static AnalyzerResult Build(AnalyzerManager manager, Project project, string tfm)
        {
            Console.WriteLine("Building {0} ({1})...", Path.GetFileName(project.Path), tfm);

            var projectAnalyzer = manager.GetProject(project.Path);
            var results = (IEnumerable<AnalyzerResult>)projectAnalyzer.Build();

            if (!string.IsNullOrWhiteSpace(tfm))
            {
                var closest = GetClosestTargetFramework(tfm, results.Select(x => x.TargetFramework));
                results = results.Where(p => p.TargetFramework.Equals(closest, StringComparison.OrdinalIgnoreCase));
            }

            return results.FirstOrDefault();
        }

        private static string GetClosestTargetFramework(string tfm, IEnumerable<string> other)
        {
            var provider = DefaultFrameworkNameProvider.Instance;
            var current = NuGetFramework.Parse(tfm, provider);

            var mappings = new Dictionary<NuGetFramework, string>(
                other.ToDictionary(x => NuGetFramework.Parse(x, provider), y => y,
                new NuGetFrameworkFullComparer()));

            var reducer = new FrameworkReducer();
            var nearest = reducer.GetNearest(current, mappings.Keys);

            return mappings[nearest];
        }
    }
}
