using System.Diagnostics;

namespace Snitch.Analyzing
{
    [DebuggerDisplay("{Name,nq} ({Version,nq})")]
    public sealed class Package
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }
}
