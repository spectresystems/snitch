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
            // An exact version is the same thing as the range it implies, so comparing
            // both sides as ranges also covers a reference declaring '1.0.0' next to
            // one declaring '[1.0.0, )'.
            var left = AsRange();
            var right = package.AsRange();

            if (left == null && right == null)
            {
                // Neither reference carries a version, so a central one governs both.
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return new VersionRangeComparer().Equals(left, right);
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

            if (string.IsNullOrWhiteSpace(version))
            {
                // No version at all. Central Package Management leaves the version
                // off the reference itself, so this is expected rather than an error.
                Version = null;
                Range = null;
            }
            else if (NuGetVersion.TryParse(version, out var semanticVersion))
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
                throw new ArgumentException($"Version '{version}' for package '{name}' is not valid.", nameof(version));
            }
        }

        private VersionRange? AsRange()
        {
            if (Range != null)
            {
                return Range;
            }

            return Version != null ? new VersionRange(Version) : null;
        }
    }
}