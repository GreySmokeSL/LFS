using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Helper;
using Core.Helper;
using Domain.Enums;
using static Domain.Constants;

namespace BLL.Log
{
    public sealed class Logger : ILogger
    {
        #region Singleton

        private static BlockingCollection<string> _blockingCollection;
        private static Task _task;
        private string _logFile;

        private static readonly Lazy<Logger> Lazy = new Lazy<Logger>(() => new Logger());

        public static Logger Instance => Lazy.Value;
        public LogTarget DefaultLogTarget { get; set; } = LogTarget.All;

        private Logger()
        {
            LogFile = Path.Combine(
                        Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TestLog")).FullName,
                        $"log{DateTime.Now.ToString(DefaultDateTimeStampFormat)}.txt");
        }

        #endregion Singleton

        public string LogFile
        {
            get => _logFile;
            set { FinishLog(); _logFile = value; RunLog(); }
        }

        public Action<string> UILogAction { get; set; }

        public void Log(string message, bool addCurrentTime = true, LogTarget target = LogTarget.None)
        {
            var text = (addCurrentTime ? DateTime.Now + ": " : "") + message;
            if (target == LogTarget.None)
                target = DefaultLogTarget;
            switch (target)
            {
                case LogTarget.Console:
                    if (Console.OpenStandardInput(1) != Stream.Null)
                        Console.WriteLine(text); break;
                case LogTarget.File: _blockingCollection?.Add(text); break;
                case LogTarget.UI: UILogAction?.Invoke(text); break;
                case LogTarget.All:
                    foreach (var trg in EnumHelper.EnumToEnumerable<LogTarget>().Where(x => x > 0))
                        Log(message, addCurrentTime, trg);
                    break;
                case LogTarget.None: break;
                default: throw new ArgumentException("Unknown target " + DefaultLogTarget + ", text:" + text);
            }
        }

        public void LogException(string message, Exception e)
        {
            /*catch (OperationCanceledException e)
               {
               logger?.LogException("TransferSortDataToFileAsync cancelled", e);
               }
               catch (AggregateException ae)
               {
               if (ae.InnerExceptions != null)
               {
               var counter = 0;
               foreach (Exception e in ae.InnerExceptions)
               logger?.LogException($"TransferSortDataToFileAsync failed, error#{++counter}:", e);
               }
               }*/
            Log(string.Join(NL, message, e?.Message, e?.InnerException?.Message));
        }

        public void LogMemory() => Log(SystemResourcesHelper.GetAppUsedMemoryInfo() + " | " + SystemResourcesHelper.GetMemoryInfo());

        private void FinishLog()
        {
            _blockingCollection?.CompleteAdding();
            _task?.Wait();
        }

        private void RunLog()
        {
            _blockingCollection = new BlockingCollection<string>();
            _task = Task.Factory.StartNew(() =>
                {
                    using (var streamWriter = new StreamWriter(LogFile, true, Encoding.UTF8) { AutoFlush = true })
                    {
                        foreach (var s in _blockingCollection.GetConsumingEnumerable())
                            streamWriter.WriteLine(s);
                    }
                },
                TaskCreationOptions.LongRunning);
        }
    }
}
