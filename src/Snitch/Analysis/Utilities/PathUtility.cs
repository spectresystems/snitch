using System;
using System.IO;

namespace Snitch.Analysis.Utilities
{
    internal static class PathUtility
    {
        public static string GetPathRelativeToProject(Project root, string path)
        {
            var rootPath = Path.GetDirectoryName(root.Path);
            if (rootPath == null)
            {
                throw new InvalidOperationException("Could not get projet root path.");
            }

            return Path.GetFullPath(Path.Combine(rootPath, path));
        }

        public static string GetProjectPath(string? path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                path = Path.GetFullPath(path);
                if (!File.Exists(path))
                {
                    if (Directory.Exists(path))
                    {
                        return FindProject(path);
                    }

                    throw new InvalidOperationException("Project do not exist.");
                }

                return path;
            }
            else
            {
                return FindProject();
            }
        }

        private static string FindProject(string? root = null)
        {
            root ??= Environment.CurrentDirectory;

            var projects = Directory.GetFiles(root, "*.csproj");
            if (projects.Length == 0)
            {
                throw new InvalidOperationException("No project file found.");
            }

            if (projects.Length > 1)
            {
                throw new InvalidOperationException("More than one project file found.");
            }

            return projects[0];
        }
    }
}
