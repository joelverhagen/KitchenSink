using System;
using System.Threading;
using System.Threading.Tasks;
using Knapcode.KitchenSink.Support;

namespace Knapcode.KitchenSink.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Convert a modern task to a legacy APM result.
        /// From http://blogs.msdn.com/b/pfxteam/archive/2011/06/27/10179452.aspx
        /// </summary>
        /// <typeparam name="T">The return type of the task.</typeparam>
        /// <param name="task">The task.</param>
        /// <param name="callback">The APM callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>The task.</returns>
        public static Task<T> ToApm<T>(this Task<T> task, AsyncCallback callback, object state)
        {
            Guard.ArgumentNotNull(task, "task");

            if (task.AsyncState == state)
            {
                if (callback != null)
                {
                    task.ContinueWith((t, cb) => ((AsyncCallback) cb)(t), callback, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
                }

                return task;
            }

            var tcs = new TaskCompletionSource<T>(state);

            task.ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.TrySetException(t.Exception.InnerExceptions);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(t.Result);
                    }

                    if (callback != null)
                    {
                        callback(tcs.Task);
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

            return tcs.Task;
        }

        /// <summary>
        /// Convert a modern task to a legacy APM result.
        /// From http://blogs.msdn.com/b/pfxteam/archive/2011/06/27/10179452.aspx
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="callback">The APM callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>The task.</returns>
        public static Task ToApm(this Task task, AsyncCallback callback, object state)
        {
            Guard.ArgumentNotNull(task, "task");

            if (task.AsyncState == state)
            {
                if (callback != null)
                {
                    task.ContinueWith((t, c) => ((AsyncCallback) c)(t), callback, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
                }

                return task;
            }

            var tcs = new TaskCompletionSource<object>(state);

            task.ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.TrySetException(t.Exception.InnerExceptions);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(null);
                    }

                    if (callback != null)
                    {
                        callback(tcs.Task);
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

            return tcs.Task;
        }

        public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
        {
            var cts = new CancellationTokenSource();
            var firstTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token));
            if (firstTask == task)
            {
                cts.Cancel();
                return await task;
            }

            throw new TimeoutException();
        }

        public static async Task WithTimeout(this Task task, TimeSpan timeout)
        {
            var cts = new CancellationTokenSource();
            var firstTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token));
            if (firstTask == task)
            {
                cts.Cancel();
                return;
            }

            throw new TimeoutException();
        }
    }
}