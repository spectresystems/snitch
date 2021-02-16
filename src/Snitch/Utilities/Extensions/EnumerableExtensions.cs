using System;
using System.Collections.Generic;

namespace Snitch
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(int index, bool first, bool last, T item)> Enumerate<T>(this IEnumerable<T> source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return Enumerate(source.GetEnumerator());
        }

        public static IEnumerable<(int index, bool first, bool last, T item)> Enumerate<T>(this IEnumerator<T> source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var first = true;
            var last = !source.MoveNext();
            T current;

            for (var index = 0; !last; index++)
            {
                current = source.Current;
                last = !source.MoveNext();
                yield return (index, first, last, current);
                first = false;
            }
        }
    }
}
