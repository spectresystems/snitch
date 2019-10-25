using System.Collections.Generic;

namespace Snitch.Analysis
{
    public static class ProjectAnalyzer
    {
        public static ProjectAnalyzerResult Analyze(Project project)
        {
            var result = new List<PackageToRemove>();
            AnalyzeProject(project, result);
            return new ProjectAnalyzerResult(result);
        }

        private static List<ProjectPackage> AnalyzeProject(Project project, List<PackageToRemove> result)
        {
            var accumulated = new List<ProjectPackage>();

            if (project.ProjectReferences.Count > 0)
            {
                // Iterate through all project references.
                foreach (var child in project.ProjectReferences)
                {
                    // Analyze the project recursively.
                    var childResult = AnalyzeProject(child, result);
                    foreach (var item in childResult)
                    {
                        // Didn't exist previously in the list of accumulated packages?
                        if (!accumulated.ContainsPackage(item.Package))
                        {
                            accumulated.Add(new ProjectPackage(child, item.Package));
                        }
                    }
                }

                // Was any package in the current project references
                // by one of the projects referenced by the project?
                foreach (var package in project.Packages)
                {
                    var found = accumulated.FindProjectPackage(package);
                    if (found != null)
                    {
                        if (!result.ContainsPackage(found.Package))
                        {
                            result.Add(new PackageToRemove(project, package, found));
                        }
                    }
                    else
                    {
                        // Add the package to the list of accumulated packages.
                        accumulated.Add(new ProjectPackage(project, package));
                    }
                }
            }
            else
            {
                foreach (var item in project.Packages)
                {
                    if (!accumulated.ContainsPackage(item))
                    {
                        accumulated.Add(new ProjectPackage(project, item));
                    }
                }
            }

            return accumulated;
        }
    }
}
