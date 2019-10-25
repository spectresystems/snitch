using System.Collections.Generic;
using System.Linq;

namespace Snitch.Analysis
{
    public sealed class ProjectAnalyzerResult
    {
        public IReadOnlyList<PackageToRemove> CanBeRemoved { get; }
        public IReadOnlyList<PackageToRemove> MightBeRemoved { get; }

        public bool NoPackagesToRemove => CanBeRemoved.Count == 0 && MightBeRemoved.Count == 0;

        public ProjectAnalyzerResult(IEnumerable<PackageToRemove> packages)
        {
            CanBeRemoved = new List<PackageToRemove>(packages.Where(p => p.CanBeRemoved));
            MightBeRemoved = new List<PackageToRemove>(packages.Where(p => p.VersionMismatch));
        }
    }
}
