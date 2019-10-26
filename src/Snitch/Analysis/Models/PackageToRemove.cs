using System;
using System.Diagnostics;

namespace Snitch.Analysis
{
    [DebuggerDisplay("{PackageDescription(),nq}")]
    internal sealed class PackageToRemove
    {
        public Project Project { get; }
        public Package Package { get; }
        public ProjectPackage Original { get; }

        public bool CanBeRemoved => Package.Version == Original.Package.Version;
        public bool VersionMismatch => Package.Version != Original.Package.Version;

        public PackageToRemove(Project project, Package package, ProjectPackage original)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
            Package = package ?? throw new ArgumentNullException(nameof(package));
            Original = original ?? throw new ArgumentNullException(nameof(original));
        }

        private string PackageDescription()
        {
            return $"{Project.Name}: {Package.Name} ({Original.Project.Name})";
        }
    }
}
