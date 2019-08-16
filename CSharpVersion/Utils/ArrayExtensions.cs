using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace EmailReportFunction.Utils
{
    public static class ArrayExtensions
    {
        public static TResult[] ParallelSelectOnArray<TSource, TResult>(this TSource[] source, Func<TSource, TResult> body)
        {
            var length = source.Length;
            var results = new TResult[length];

            Parallel.For(0, length, pos => { results[pos] = body(source[pos]); });

            return results;
        }

        public static async Task<TResult[]> ParallelSelectOnArrayAsync<TSource, TResult>(this TSource[] source, Func<TSource, Task<TResult>> body)
        {
            var results = new TResult[source.Length];
            var worker = new ActionBlock<int>(async index =>
            {
                var result = await body(source[index]).ConfigureAwait(false);
                if (result != null)
                {
                    results[index] = result;
                }
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 * Environment.ProcessorCount });

            for (int i = 0; i < source.Length; i++)
            {
                worker.Post(i);
            }

            worker.Complete();
            await worker.Completion.ConfigureAwait(false);
            return results;
        }

        public static void ForEach<TSource>(this TSource[] source, Action<TSource> action)
        {
            Array.ForEach(source, action);
        }
    }
}
