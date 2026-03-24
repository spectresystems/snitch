using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Snitch.Analysis.Utilities
{
    internal static class SlnxParser
    {
        public static List<string> GetProjectsFromSlnx(string slnxPath)
        {
            if (!File.Exists(slnxPath))
            {
                throw new FileNotFoundException($"Solution file not found: {slnxPath}");
            }

            var slnxDirectory = Path.GetDirectoryName(slnxPath)
                ?? throw new InvalidOperationException("Could not determine solution directory.");

            var doc = XDocument.Load(slnxPath);
            var solution = doc.Root;

            if (solution == null || solution.Name.LocalName != "Solution")
            {
                throw new InvalidOperationException("Invalid slnx file: missing Solution root element.");
            }

            var projects = new List<string>();
            CollectProjects(solution, slnxDirectory, projects);

            return projects;
        }

        private static void CollectProjects(XElement element, string slnxDirectory, List<string> projects)
        {
            foreach (var child in element.Elements())
            {
                if (child.Name.LocalName == "Project")
                {
                    var pathAttr = child.Attribute("Path");
                    if (pathAttr != null && !string.IsNullOrWhiteSpace(pathAttr.Value))
                    {
                        var projectPath = pathAttr.Value;

                        // Only include MSBuild project files (.csproj, .fsproj, .vbproj)
                        if (IsMSBuildProject(projectPath))
                        {
                            var absolutePath = Path.GetFullPath(Path.Combine(slnxDirectory, projectPath));
                            if (!projects.Contains(absolutePath))
                            {
                                projects.Add(absolutePath);
                            }
                        }
                    }
                }
                else if (child.Name.LocalName == "Folder")
                {
                    // Recursively collect projects from folders
                    CollectProjects(child, slnxDirectory, projects);
                }
            }
        }

        private static bool IsMSBuildProject(string path)
        {
            return path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".fsproj", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase);
        }
    }
}
