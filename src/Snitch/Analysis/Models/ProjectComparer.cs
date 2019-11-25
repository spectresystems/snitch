using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            return x.Path.Equals(y.Path, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] Project obj)
        {
            return obj?.Path?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0;
        }
    }
}
