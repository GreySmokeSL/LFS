using System;
using Core.Helper;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static Domain.Constants;

namespace BLL.FileStream
{
    public class ObservableFileWriter
    {
        private readonly BlockingCollection<string> _blockingCollection = new BlockingCollection<string>();
        private readonly CancellationToken _ct;
        private readonly Task _task;

        public ulong AddedCounter { get; private set; }
        public ulong WrittenCounter { get; private set; }
        public int CurrentCapacity => _blockingCollection.Count;

        public ObservableFileWriter(string targetFile, ulong maxCapacity = 0, CancellationToken? ct = null)
        {
            _ct = ct ?? new CancellationToken();
            if (maxCapacity == 0)
                maxCapacity = ulong.MaxValue;
            _task = Task.Factory.StartNew(() =>
                {
                    using (var sw = new StreamWriter(targetFile, true, DefaultFileStreamEncoding, WriteFileStreamBufferSize))
                    {
                        try
                        {
                            foreach (var s in _blockingCollection.GetConsumingEnumerable(_ct).Take(maxCapacity))
                            {
                                sw.WriteLine(s);
                                WrittenCounter++;
                            }
                        }
                        catch (OperationCanceledException)
                        {

                        }
                        finally
                        {
                            sw.Flush();
                        }
                    }
                }, _ct, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void AddItem(string text)
        {
            try
            {
                _blockingCollection.Add(text, _ct);
                AddedCounter++;
            }
            catch (OperationCanceledException)
            {

            }
        }

        public ulong Complete()
        {
            _blockingCollection.CompleteAdding();
            _task?.Wait(_ct);
            return WrittenCounter;
        }
    }
}
