using System;
using System.Diagnostics;
using NuGet.Versioning;

namespace Snitch.Analysis
{
    [DebuggerDisplay("{Name,nq} ({Version,nq})")]
    internal sealed class Package
    {
        public string Name { get; }
        public NuGetVersion Version { get; }

        public Package(string name, string version)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = ParseSemanticVersion(version ?? throw new ArgumentNullException(nameof(version)), Name);

            static NuGetVersion ParseSemanticVersion(string version, string name)
            {
                if (!NuGetVersion.TryParse(version, out var semanticVersion))
                {
                    throw new ArgumentException($"Version '{version}' for package '{name}' is not valid.", nameof(version));
                }

                return semanticVersion;
            }
        }
    }
}
