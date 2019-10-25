using System.Diagnostics;

namespace Snitch.Analyzing
{
    [DebuggerDisplay("{PackageDescription(),nq}")]
    public sealed class ProjectPackage
    {
        public Project Project { get; set; }
        public Package Package { get; set; }

        private string PackageDescription()
        {
            return $"{Project.Filename}: {Package.Name}";
        }
    }
}
