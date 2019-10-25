using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Snitch.Analyzing
{

    public static class Analyzer
    {
        public static IReadOnlyList<PackageToRemove> Analyze(Project project)
        {
            var result = new List<PackageToRemove>();
            AnalyzeProject(project, result);
            return result;
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
                        // Didn't exist previously in the list of 
                        // accumulated packages?
                        if (!accumulated.Any(x => x.Package.Name.Equals(item.Package.Name)))
                        {
                            accumulated.Add(new ProjectPackage
                            {
                                Package = item.Package,
                                Project = child
                            });
                        }
                    }
                }

                // Was any package in the current project references
                // by one of the projects referenced by the project?
                foreach (var package in project.Packages)
                {
                    var found = accumulated.FirstOrDefault(p => p.Package.Name.Equals(package.Name, StringComparison.OrdinalIgnoreCase));
                    if (found != null)
                    {
                        if(!result.Any(x => x.Package.Package.Name.Equals(found.Package.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            result.Add(new PackageToRemove
                            {
                                Package = new ProjectPackage
                                {
                                    Project = project,
                                    Package = package,
                                },
                                OriginalLocation = found
                            });
                        }
                    }
                    else
                    {
                        accumulated.Add(new ProjectPackage
                        {
                            Project = project,
                            Package = package
                        });
                    }
                }
            }
            else
            {
                foreach (var item in project.Packages)
                {
                    if (!accumulated.Any(x => x.Package.Name.Equals(item.Name)))
                    {
                        accumulated.Add(new ProjectPackage
                        {
                            Project = project,
                            Package = item
                        });
                    }
                }
            }

            return accumulated;
        }
    }
}
