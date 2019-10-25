using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Buildalyzer;
using Snitch.Analysis.Utilities;

namespace Snitch.Analysis
{
    public static class ProjectBuilder
    {
        public static Project Build(string path, string? tfm)
        {
            Console.Write("Analysing project ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(Path.GetFileNameWithoutExtension(path));
            Console.ResetColor();
            Console.WriteLine("...\n");

            var manager = new AnalyzerManager();
            var built = new Dictionary<string, Project>(StringComparer.OrdinalIgnoreCase);

            return Analyze(manager, path, tfm, built);
        }

        public static Project Analyze(AnalyzerManager manager, string path, string? tfm, Dictionary<string, Project> built)
        {
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
                throw new InvalidOperationException($"Could not build {path}");
            }

            // Set the target framework.
            project.TargetFramework = result.TargetFramework;

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
                var analyzedProjectReference = Analyze(manager, projectReferencePath, project.TargetFramework, built);

                project.ProjectReferences.Add(analyzedProjectReference);
            }

            return project;
        }

        private static AnalyzerResult Build(AnalyzerManager manager, Project project, string? tfm)
        {
            Console.Write("Building ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(project.Name);
            Console.ResetColor();
            Console.Write(" (");
            Console.Write(tfm ?? "?");
            Console.Write(")");
            Console.WriteLine("...");

            var projectAnalyzer = manager.GetProject(project.Path);
            var results = (IEnumerable<AnalyzerResult>)projectAnalyzer.Build();

            if (!string.IsNullOrWhiteSpace(tfm))
            {
                var closest = results.GetNearestFrameworkMoniker(tfm);
                results = results.Where(p => p.TargetFramework.Equals(closest, StringComparison.OrdinalIgnoreCase));
            }

            return results.FirstOrDefault();
        }
    }
}
