using System.Diagnostics;

namespace Snitch.Analyzing
{
    [DebuggerDisplay("{PackageDescription(),nq}")]
    public sealed class PackageToRemove
    {
        public ProjectPackage Package { get; set; }
        public ProjectPackage OriginalLocation { get; set;  }

        // HACK: Compare semver at some point...
        public bool CanBeRemoved => Package.Package.Version == OriginalLocation.Package.Version;
        public bool VersionMismatch => Package.Package.Version != OriginalLocation.Package.Version;

        private string PackageDescription()
        {
            return $"{Package.Project.Filename}: {Package.Package.Name} ({OriginalLocation.Project.Filename})";
        }
    }
}
