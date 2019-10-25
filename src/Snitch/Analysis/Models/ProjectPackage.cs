using System;
using System.Diagnostics;

namespace Snitch.Analysis
{
    [DebuggerDisplay("{PackageDescription(),nq}")]
    public sealed class ProjectPackage
    {
        public Project Project { get; }
        public Package Package { get; }

        public ProjectPackage(Project project, Package package)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
            Package = package ?? throw new ArgumentNullException(nameof(package));
        }

        private string PackageDescription()
        {
            return $"{Project.Name}: {Package.Name}";
        }
    }
}
