using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Snitch.Analysis
{
    [DebuggerDisplay("{GetProjectName(),nq}")]
    internal sealed class Project
    {
        public string Path { get; }
        public string Name { get; }
        public string TargetFramework { get; set; }
        public string? LockFilePath { get; set; }
        public List<Project> ProjectReferences { get; }
        public List<Package> Packages { get; }

        /// <summary>
        /// Gets the target framework the project was asked to build for, which is
        /// empty when none was given. A multi targeting project yields different
        /// packages per framework, so this is part of its identity.
        /// </summary>
        public string RequestedTargetFramework { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the project targets more than one
        /// framework. A project that targets exactly one builds the same way whichever
        /// framework was asked for, so it can be reused for any of them.
        /// </summary>
        public bool IsMultiTargeting { get; set; }

        public Project(string path, string? requestedTargetFramework = null)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Name = System.IO.Path.GetFileNameWithoutExtension(Path);
            TargetFramework = string.Empty;
            RequestedTargetFramework = requestedTargetFramework ?? string.Empty;
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