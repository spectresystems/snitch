using System;
using System.Diagnostics;
using NuGet.Versioning;

namespace Snitch.Analysis
{
    [DebuggerDisplay("{Name,nq} ({Version,nq})")]
    public sealed class Package
    {
        public string Name { get; }
        public NuGetVersion? Version { get; set; }
        public VersionRange? Range { get; }

        public bool IsGreaterThan(Package package, out bool indeterminate)
        {
            indeterminate = true;

            if (Version != null && package.Version != null)
            {
                // Version > Version
                indeterminate = false;
                return new VersionComparer().Compare(Version, package.Version) > 0;
            }
            else if (Version != null && package.Range != null)
            {
                // Version > Range
                return package.Range.Satisfies(Version);
            }
            else if (Range != null && package.Range != null)
            {
                // Range > Range
                indeterminate = false;
                return new VersionComparer().Compare(Range.MaxVersion, package.Range.MaxVersion) > 0;
            }
            else if (Range != null && package.Version != null)
            {
                // Range > Version
                return Range.Satisfies(package.Version);
            }

            return false;
        }

        public bool IsSameVersion(Package package)
        {
            if (Version != null && package.Version != null)
            {
                // Version == Version
                return new VersionComparer().Equals(Version, package.Version);
            }
            else if (Range != null && package.Range != null)
            {
                // Range == Range
                new VersionRangeComparer().Equals(Range, package.Range);
            }

            return false;
        }

        public string GetVersionString()
        {
            if (Version != null)
            {
                return Version.ToString();
            }

            return Range?.OriginalString ?? "?";
        }

        public Package(string name, string version)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            if (NuGetVersion.TryParse(version, out var semanticVersion))
            {
                Version = semanticVersion;
                Range = null;
            }
            else
            {
                if (!VersionRange.TryParse(version, out var range))
                {
                    throw new ArgumentException($"Version '{version}' for package '{name}' is not valid.", nameof(version));
                }

                Version = null;
                Range = range;
            }
        }
    }
}
