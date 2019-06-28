using EmailReportFunction.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.Utils
{
    /// <summary>
    /// This class has extension menthods that are not in Linq
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Returns the max object using the value for each item returned by the given selector.
        /// (Similar to .Max() from Linq, but this returns the object itself instead of the max value.)
        /// </summary>
        public static TSource TakeMax<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            TSource currentMaxItem = default(TSource);
            var currentMaxValue = int.MinValue;
            var hasAtLeastSetOnce = false;

            foreach (TSource item in source)
            {
                var value = selector(item);
                if (!hasAtLeastSetOnce || value > currentMaxValue)
                {
                    hasAtLeastSetOnce = true;
                    currentMaxValue = value;
                    currentMaxItem = item;
                }
            }

            return currentMaxItem;
        }

        public static List<TSource> Merge<TSource>(this IEnumerable<IEnumerable<TSource>> source)
        {
            List<TSource> mergedResults = new List<TSource>();

            foreach (var item in source)
            {
                if (item != null)
                {
                    mergedResults.AddRange(item);
                }
            }

            return mergedResults;
        }

        public static List<List<TSource>> Split<TSource>(this IEnumerable<TSource> source, int splitSize)
        {
            if (splitSize <= 0)
            {
                throw new EmailReportException($"Cannot have split size less than one. passed split size - {splitSize}");
            }

            List<TSource> sourceList = new List<TSource>(source);
            List<List<TSource>> chunks = new List<List<TSource>>();

            if (!sourceList.Any())
            {
                return chunks;
            }

            if (splitSize > sourceList.Count)
            {
                chunks.Add(new List<TSource>(sourceList));
            }
            else
            {
                List<TSource> currentChunk = null;

                foreach (TSource item in sourceList)
                {
                    if (currentChunk == null || currentChunk.Count >= splitSize)
                    {
                        currentChunk = new List<TSource>();
                        chunks.Add(currentChunk);
                    }

                    currentChunk.Add(item);
                }
            }

            return chunks;
        }

        public static TResult[] ParallelSelect<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> body)
        {
            return source.ToArray().ParallelSelectOnArray(body);
        }

        public async static Task<TResult[]> ParallelSelectAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> body)
        {
            return await source.ToArray().ParallelSelectOnArrayAsync(body).ConfigureAwait(false);
        }

        public static IEnumerable<TResult> DistinctBy<TResult>(this IEnumerable<TResult> source,
            Func<TResult, object> selector)
        {
            return source.GroupBy(selector).Select(g => g.First());
        }
    }
}
