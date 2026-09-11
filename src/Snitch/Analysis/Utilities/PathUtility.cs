using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Construction;

namespace Snitch.Analysis.Utilities
{
    internal static class PathUtility
    {
        public static string GetPathRelativeToProject(Project root, string path)
        {
            var rootPath = Path.GetDirectoryName(root.Path);
            if (rootPath == null)
            {
                throw new InvalidOperationException("Could not get project root path.");
            }

            return Path.GetFullPath(Path.Combine(rootPath, path));
        }

        public static List<string> GetProjectPaths(string? path, out string entry)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                path = Path.GetFullPath(path);
                if (!File.Exists(path))
                {
                    if (Directory.Exists(path))
                    {
                        return FindProjects(path, out entry);
                    }

                    throw new InvalidOperationException("Project or solution file do not exist.");
                }

                entry = path;
                return GetProjectsFromFile(path);
            }

            return FindProjects(null, out entry);
        }

        private static List<string> GetProjectsFromFile(string path)
        {
            if (path.EndsWith("proj", StringComparison.InvariantCulture))
            {
                return new List<string> { path };
            }

            if (path.EndsWith(".sln", StringComparison.InvariantCulture))
            {
                return GetProjectsFromSolution(path);
            }

            if (path.EndsWith(".slnx", StringComparison.InvariantCulture))
            {
                return GetProjectsFromSlnxSolution(path);
            }

            throw new InvalidOperationException("Project or solution file do not exist.");
        }

        private static List<string> FindProjects(string? root, out string entry)
        {
            root ??= Environment.CurrentDirectory;

            var slns = Directory.GetFiles(root, "*.sln").Concat(Directory.GetFiles(root, "*.slnx")).ToArray();

            if (slns.Length == 0)
            {
                // F# projects are analyzed just like C# ones, so discover them too.
                var subProjects = Directory.GetFiles(root, "*.csproj").Concat(Directory.GetFiles(root, "*.fsproj")).ToArray();
                if (subProjects.Length == 0)
                {
                    throw new InvalidOperationException("No project or solution file found.");
                }
                else if (subProjects.Length > 1)
                {
                    throw new InvalidOperationException("More than one project file found.");
                }

                entry = subProjects[0];
                return new List<string>(new[] { subProjects[0] });
            }
            else if (slns.Length > 1)
            {
                throw new InvalidOperationException("More than one solution file found.");
            }
            else
            {
                entry = slns[0];
                return GetProjectsFromFile(slns[0]);
            }
        }

        private static List<string> GetProjectsFromSolution(string solution)
        {
            var solutionFile = SolutionFile.Parse(solution);
            return solutionFile.ProjectsInOrder.Where(p => p.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat).Select(p => p.AbsolutePath).Distinct().ToList();
        }

        private static List<string> GetProjectsFromSlnxSolution(string solution)
        {
            var directory = Path.GetDirectoryName(solution) ?? Environment.CurrentDirectory;

            // Projects can be nested in folders, so look at every descendant.
            return XDocument.Load(solution).Descendants("Project")
                .Select(project => project.Attribute("Path")?.Value)
                .Where(path => path != null && path.EndsWith("proj", StringComparison.OrdinalIgnoreCase))
                .Select(path => Path.GetFullPath(Path.Combine(directory, path!.Replace('\\', Path.DirectorySeparatorChar))))
                .Distinct()
                .ToList();
        }
    }
}