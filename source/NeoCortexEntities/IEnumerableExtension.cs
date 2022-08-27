using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoCortexApi.Entities
{
    public static class IEnumerableExtension
    {
        public static bool ElementsEqual<T>(this IEnumerable<T> source, IEnumerable<T> dest)
        {
            return DefaultCheck(source, dest, () =>
            {
                var list = source.Except(dest);
                return list.Any() == false;
            });
        }

        public static bool KeyValuesEqual<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> dest)
        {
            return DefaultCheck(source, dest, () =>
            {
                foreach (var key in source.Keys)
                {
                    if (dest.TryGetValue(key, out TValue value) == false)
                        return false;

                    if (!source[key].Equals(value))
                        return false;
                }
                return true;
            });
        }

        public static bool TryIfSequenceEqual<T>(this IEnumerable<T> source, IEnumerable<T> dest)
        {
            return DefaultCheck<T>(source, dest, () => source.SequenceEqual(dest));
        }
        private static bool DefaultCheck<T>(this IEnumerable<T> source, IEnumerable<T> dest, Func<bool> compareMethod)
        {
            if (source == null && dest != null ||
                source != null && dest == null)
            {
                return false;
            }
            else if (source == null && dest == null)
                return true;
            else if (source.Count() == 0)
                return dest.Count() == 0;
            else
            {
                return compareMethod();
            }
        }
    }
}
