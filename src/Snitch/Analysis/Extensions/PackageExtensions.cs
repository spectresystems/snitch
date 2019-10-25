using System;
using System.Collections.Generic;
using System.Linq;

namespace Snitch.Analysis
{
    public static class PackageExtensions
    {
        public static bool ContainsPackage(this IEnumerable<ProjectPackage> source, Package package)
        {
            return source.Any(x => x.Package.Name.Equals(package.Name));
        }

        public static ProjectPackage FindProjectPackage(this IEnumerable<ProjectPackage> source, Package package)
        {
            return source.FirstOrDefault(p => p.Package.Name.Equals(package.Name, StringComparison.OrdinalIgnoreCase));
        }

        public static bool ContainsPackage(this IEnumerable<PackageToRemove> source, Package package)
        {
            return source.Any(x => x.Package.Name.Equals(package.Name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
