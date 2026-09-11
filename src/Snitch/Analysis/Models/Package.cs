using System;
using System.Diagnostics;
using System.Linq;
using NuGet.Versioning;

namespace Snitch.Analysis
{
    [DebuggerDisplay("{Name,nq} ({Version,nq})")]
    internal sealed class Package
    {
        public string Name { get; }
        public NuGetVersion? Version { get; set; }
        public VersionRange? Range { get; }

        public string? PrivateAssets { get; }

        /// <summary>
        /// Gets a value indicating whether the compile time assets of this package
        /// are kept from projects referencing the one declaring it.
        /// </summary>
        public bool IsPrivate => PrivateAssets != null && PrivateAssets
            .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Any(asset =>
                asset.Trim().Equals("compile", StringComparison.OrdinalIgnoreCase) ||
                asset.Trim().Equals("all", StringComparison.OrdinalIgnoreCase));

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
            if (Version == null && Range == null && package.Version == null && package.Range == null)
            {
                // Neither reference carries a version, so a central one governs both.
                return true;
            }
            else if (Version != null && package.Version != null)
            {
                // Version == Version
                return new VersionComparer().Equals(Version, package.Version);
            }
            else if (Range != null && package.Range != null)
            {
                // Range == Range
                return new VersionRangeComparer().Equals(Range, package.Range);
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

        public Package(string name, string? version, string? privateAssets)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PrivateAssets = privateAssets;

            if (NuGetVersion.TryParse(version, out var semanticVersion))
            {
                Version = semanticVersion;
                Range = null;
            }
            else if (VersionRange.TryParse(version, out var range))
            {
                Version = null;
                Range = range;
            }
            else
            {
                // No version at all. Central Package Management leaves the version
                // off the reference itself, so this is expected rather than an error.
                Version = null;
                Range = null;
            }
        }
    }
}