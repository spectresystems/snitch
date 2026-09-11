using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Snitch.Analysis
{
    internal sealed class ProjectComparer : IEqualityComparer<Project>
    {
        public bool Equals([AllowNull] Project x, [AllowNull] Project y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.Path.Equals(y.Path, StringComparison.OrdinalIgnoreCase)
                && x.RequestedTargetFramework.Equals(y.RequestedTargetFramework, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] Project obj)
        {
            if (obj?.Path == null)
            {
                return 0;
            }

            return HashCode.Combine(
                obj.Path.GetHashCode(StringComparison.OrdinalIgnoreCase),
                obj.RequestedTargetFramework.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }
    }
}