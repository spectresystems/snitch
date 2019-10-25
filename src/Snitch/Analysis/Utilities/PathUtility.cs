using System;
using System.IO;

namespace Snitch.Analysis.Utilities
{
    public static class PathUtility
    {
        public static string GetPathRelativeToProject(Project root, string path)
        {
            var rootPath = Path.GetDirectoryName(root.Path);
            if (rootPath == null)
            {
                throw new ArgumentNullException("Could not get projet root path.");
            }

            return Path.GetFullPath(Path.Combine(rootPath, path));
        }
    }
}
