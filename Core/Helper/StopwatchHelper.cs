using System;
using System.Diagnostics;

namespace Core.Helper
{
    public static class StopwatchHelper
    {
        public static StopwatchResult<T> GetWithStopwatch<T>(Func<T> getFunc, string actionName)
        {
            var stopwatch = new Stopwatch();
#if DEBUG
            stopwatch.Start();
#endif
            var result = getFunc();
#if DEBUG
            stopwatch.Stop();
#endif
            return new StopwatchResult<T>(actionName, stopwatch.Elapsed, result);
        }

        public static StopwatchResult<object> ExecWithStopwatch(Action action, string actionName)
        {
            return GetWithStopwatch<object>(() => { action(); return null; }, actionName);
        }
    }

    public class StopwatchResult<T>
    {
        public StopwatchResult(string actionName, TimeSpan elapsed, T result = default)
        {
            Result = result;
            Elapsed = elapsed;
            ActionName = actionName;
        }

        public string ActionName { get; }
        public TimeSpan Elapsed { get; }
        public T Result { get; }
        public string Description => $"{ActionName} elapsed {Elapsed}";
        public string DescriptionWithTime => $"{DateTime.Now}: {Description}";
    }
}
