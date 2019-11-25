using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Snitch.Analysis
{
    [DebuggerDisplay("{GetProjectName(),nq}")]
    internal sealed class Project
    {
        public string Path { get; }
        public string File { get; }
        public string Name { get; }
        public string TargetFramework { get; set; }
        public string? LockFilePath { get; set; }
        public List<Project> ProjectReferences { get; }
        public List<Package> Packages { get; }

        public Project(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            File = System.IO.Path.GetFileName(Path);
            Name = System.IO.Path.GetFileNameWithoutExtension(Path);
            TargetFramework = string.Empty;
            ProjectReferences = new List<Project>();
            Packages = new List<Package>();
        }

        public void RemovePackages(IEnumerable<string> packages)
        {
            if (packages != null)
            {
                foreach (var package in packages)
                {
                    RemovePackage(package);
                }
            }
        }

        private void RemovePackage(string package)
        {
            Packages.RemoveAll(p => p.Name.Equals(package, StringComparison.OrdinalIgnoreCase));
            foreach (var parentProject in ProjectReferences)
            {
                parentProject.RemovePackage(package);
            }
        }

        private string GetProjectName()
        {
            return System.IO.Path.GetFileName(Path);
        }
    }
}
