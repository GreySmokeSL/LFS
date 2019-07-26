using System;
using System.Collections.Generic;
using System.Linq;
using Core.Extensions;

namespace Core.Helper
{
    public static class EnumerableHelper
    {
        public static bool IsEmpty<T>(this IEnumerable<T> list)
        {
            return list == null || !list.Any();
        }

        public static ICollection<TSource> Materialize<TSource>(this IEnumerable<TSource> source)
        {
            switch (source)
            {
                case IList<TSource> _:
                    return (IList<TSource>)source;
                case ICollection<TSource> _:
                    return (ICollection<TSource>)source;
                default:
                    return source.ToList();
            }
        }

        public static IEnumerable<T> RandomShuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(p => p.GetHashCode());
        }

        public static IEnumerable<T> Repeat<T>(ulong count, Func<ulong, T> action)
        {
            return Range(1, count).Select(action);
        }

        public static IEnumerable<IEnumerable<T>> RepeatWhile<T>(Func<IEnumerable<T>> action, Func<bool> condition)
        {
            do
            {
                yield return action();
            }
            while (condition());
        }

        public static IEnumerable<IEnumerable<T>> WhileRepeat<T>(Func<bool> condition, Func<IEnumerable<T>> action)
        {
            while (condition())
            {
                yield return action();
            }
        }

        public static IEnumerable<T> Take<T>(this IEnumerable<T> source, ulong count)
        {
            if (count < 1 || source == null)
                yield break;
            foreach (var item in source)
            {
                yield return item;
                if (--count == 0)
                    break;
            }
        }

        public static ulong USum<TSource>(this IEnumerable<TSource> source, Func<TSource, ulong> selector) 
        {
            return source?.Aggregate((ulong)0, (s, x) => s + selector(x)) ?? 0;
        }

        public static IEnumerable<ulong> Range(ulong start, ulong count)
        {
            for (ulong i = 0; i < count; ++i)
                yield return checked(start + i);
        }

        public static IEnumerable<ushort> Range(ushort start, ushort count)
        {
            for (ushort i = 0; i < count; ++i)
                yield return (ushort)checked(start + i);
        }

        public static IEnumerable<int> FindAllIndexof<T>(this IEnumerable<T> values, T val, byte offset = 0)
        {
            return values.Select((b, i) => Equals(b, val) ? i + offset : -1 - offset).Where(i => i > 0);
        }

        public static IEnumerable<int> GetPositions<T>(this T[] x, T[] y, int delta = 0)
        {
            //Possible positions
            var index = Enumerable.Range(0, x.Length - y.Length - delta + 1);
            //add check for each array item of sequence
            return Enumerable.Range(0, y.Length).Aggregate(index, (current, ii) => current.Where(n => Equals(x[n + ii], y[ii])));
        }

        public static string Join<T>(this IEnumerable<T> source, string separator = null, bool useSeparAsPrefix = false)
        {
            if (source.IsEmpty())
                return string.Empty;

            return useSeparAsPrefix ? string.Join(string.Empty, source.Select(s => separator + (s is string ? s as string : s?.ToString())))
                : string.Join(separator, source.Select(s => s is string ? s as string : s?.ToString()));
        }

        public static IEnumerable<IEnumerable<T>> GroupWhileAggregating<T, TAccume>(
            this IEnumerable<T> source,
            TAccume seed,
            Func<TAccume, T, TAccume> accumulator,
            Func<TAccume, T, bool> predicate)
        {
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                    yield break;

                var list = new List<T> { iterator.Current };
                var accume = accumulator(seed, iterator.Current);
                while (iterator.MoveNext())
                {
                    accume = accumulator(accume, iterator.Current);
                    if (predicate(accume, iterator.Current))
                        list.Add(iterator.Current);
                    else
                    {
                        yield return list;
                        list = new List<T> { iterator.Current };
                        accume = accumulator(seed, iterator.Current);
                    }
                }
                yield return list;
            }
        }


        public static int IndexOf<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            int i = 0;
            foreach (var pair in dictionary)
            {
                if (pair.Key.Equals(key))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        public static int IndexOf<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value)
        {
            int i = 0;
            foreach (var pair in dictionary)
            {
                if (pair.Value.Equals(value))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
    }
}
