using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BLL.Log;
using Domain.Domain;

namespace BLL.Helper
{
    public static class ParallelHelper
    {
        public static void ParallelWhile(this Action action, Func<bool> condition)
        {
            Parallel.ForEach(IterateWhile(condition), body => action());
        }

        private static IEnumerable<bool> IterateWhile(Func<bool> condition)
        {
            while (condition())
                yield return true;
        }

        public static async Task<IEnumerable<TResult>> DoParallelAsync<T, TResult>(this IEnumerable<T> source, int concurrentCount,
            Func<T, TResult> method, ILogger logger = null, bool forwardException = false)
        {
            var sp = new Semaphore(concurrentCount, concurrentCount);
            var taskList = source.Select(item =>
            {
                return Task.Run(() =>
                {
                    sp.WaitOne();

                    var result = default(TResult);

                    try
                    {
                        result = method(item);
                    }
                    catch (Exception e)
                    {
                        logger?.LogException("DoParallelAsync failed on " + item, e);
                        if (forwardException)
                            throw;
                    }
                    finally
                    {
                        sp.Release();
                    }

                    return result;
                });
            });

            return await Task.WhenAll(taskList);
        }

        public static async Task<bool> AggregateParallelAsync<T, TResult, TAggr>(this IEnumerable<T> source, int maxConcurrentCount, CancellationToken cancelToken,
            Func<T, TResult> getItemResultFunc, Func<TResult, TAggr> aggregateFunc = null, Func<TAggr, bool> isCompletedPredicate = null)
        {
            return await source.AggregateParallelAsync(maxConcurrentCount, cancelToken,
                async arg => await Task.Run(() => getItemResultFunc(arg), cancelToken),
                aggregateFunc, isCompletedPredicate);
        }

        public static async Task<bool> AggregateParallelAsync<T, TResult, TAggr>(this IEnumerable<T> source, int maxConcurrentCount, CancellationToken cancelToken,
            Func<T, Task<TResult>> getItemResultAsyncFunc, Func<TResult, TAggr> aggregateFunc = null, Func<TAggr, bool> isCompletedPredicate = null)
        {
            if (source == null || maxConcurrentCount <= 0 || getItemResultAsyncFunc == null)
                throw new ArgumentException();

            cancelToken.ThrowIfCancellationRequested();

            var tasks = new List<Task<TResult>>();

            using (var nr = source.GetEnumerator())
            {
                for (var i = 0; i < maxConcurrentCount; i++)
                {
                    cancelToken.ThrowIfCancellationRequested();

                    if (nr.MoveNext())
                        tasks.Add(getItemResultAsyncFunc(nr.Current));
                }

                while (tasks.Count > 0)
                {
                    cancelToken.ThrowIfCancellationRequested();

                    var firstFinishedTask = await Task.WhenAny(tasks);
                    tasks.Remove(firstFinishedTask);

                    cancelToken.ThrowIfCancellationRequested();

                    var aggrRes = aggregateFunc != null ? aggregateFunc(await firstFinishedTask) : default(TAggr);

                    cancelToken.ThrowIfCancellationRequested();

                    if (isCompletedPredicate?.Invoke(aggrRes) ?? false)
                        break;

                    await Task.Yield();

                    cancelToken.ThrowIfCancellationRequested();

                    if (nr.MoveNext())
                        tasks.Add(getItemResultAsyncFunc(nr.Current));
                }
            }

            return true;
        }
    }
}
