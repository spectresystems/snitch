using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                return SlnxParser.GetProjectsFromSlnx(path);
            }

            throw new InvalidOperationException("Project or solution file do not exist.");
        }

        private static List<string> FindProjects(string? root, out string entry)
        {
            root ??= Environment.CurrentDirectory;

            var slns = Directory.GetFiles(root, "*.sln");
            var slnxs = Directory.GetFiles(root, "*.slnx");
            var allSolutions = new List<string>(slns);
            allSolutions.AddRange(slnxs);

            if (allSolutions.Count == 0)
            {
                var subProjects = Directory.GetFiles(root, "*.csproj");
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
            else if (allSolutions.Count > 1)
            {
                throw new InvalidOperationException("More than one solution file found.");
            }
            else
            {
                entry = allSolutions[0];
                return GetProjectsFromFile(allSolutions[0]);
            }
        }

        private static List<string> GetProjectsFromSolution(string solution)
        {
            var solutionFile = SolutionFile.Parse(solution);
            return solutionFile.ProjectsInOrder.Where(p => p.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat).Select(p => p.AbsolutePath).Distinct().ToList();
        }
    }
}