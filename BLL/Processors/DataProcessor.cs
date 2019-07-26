using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Threading.Tasks;
using BLL.Comparer;
using BLL.Data;
using Core.Extensions;
using BLL.FileStream;
using BLL.Helper;
using BLL.Log;
using Domain.Domain;
using Domain.Models;
using MoreLinq;
using static Domain.Constants;
using Core.Helper;

namespace BLL.Processors
{
    public class DataProcessor<T>
    {
        private readonly ILogger _logger;
        private readonly IProgress<ProgressInfo<int>> _progress;
        public string LinePartDelimiter { get; }
        public MultiProgressBarModel<T> ProgressModel { get; }
        private SortedSet<IndexedStreamReader> ReaderSquad { get; set; } = new SortedSet<IndexedStreamReader>();

        public DataProcessor(MultiProgressBarModel<T> progressModel, ILogger logger = null, IProgress<ProgressInfo<int>> progress = null, string linePartDelimiter = DefaultLinePartDelimiter)
        {
            _logger = logger;
            _progress = progress;
            LinePartDelimiter = linePartDelimiter;
            ProgressModel = progressModel;
        }

        public SortedSet<char> FillBaseSortedMarkers()
        {
            var _charMarkers = new SortedSet<char>();
            string.Concat(Digits, LatinLetters, CyrillicLetters).ToCharArray().ForEach(c => _charMarkers.Add(c));
            return _charMarkers;
        }

        private bool InitReaders(ArrangeDataModel model)
        {
            var pi = new ProgressInfo<long>();
            pi.PropertyChanged += (sender, args) => ProgressModel.SetMain(pi.Total, pi.Completed, pi.Description);

            var maxBlockSize = MB * (long)model.BlockSizeMb;
            var fileSize = new FileInfo(model.SourceFile).Length;
            var blockSize = new[] { maxBlockSize, 1 + (fileSize / model.MaxConcurrentCount) }.Min();

            ReaderSquad = IndexedStreamReader.SplitByBlocks(model.SourceFile, blockSize, LinePartDelimiter, pi);
            if (ReaderSquad.IsEmpty())
                return false;

            foreach (var isr in ReaderSquad)
                isr.CurrentProgress.PropertyChanged += (sender, e) =>
                {
                    var workingReaders = ReaderSquad.Where(x => x.CurrentProgress.Completed < x.CurrentProgress.Total).Select(x => x.BlockOrder).Join(", ");
                    if (workingReaders.IsNone())
                        ProgressModel.SetDetail<T>();
                    else
                        ProgressModel.SetDetail(ReaderSquad.Sum(x => x.CurrentProgress.Total / MB), ReaderSquad.Sum(x => x.CurrentProgress.Completed / MB), $"Working readers #{workingReaders}. Analyzed MB: ");
                };

            _logger?.Log($@"SplitByBlocks count={ReaderSquad.Count}, blockSize MB={blockSize / MB}, fileSize MB={new FileInfo(model.SourceFile).Length / MB}, concurrent={model.MaxConcurrentCount}, markerGroupLineCount={model.MaxMarkerGroupLineCount}");
            return true;
        }

        private async Task<SortedDictionary<string, int>> SetReaderStatisticAsync(SortedSet<IndexedStreamReader> source, int maxConcurrentCount, CancellationToken ct)
        {
            void Report(string info)
            {
                _logger?.Log("SetReaderStatistic: " + info);

                ProgressModel.UpdateStage($"Analyzing readers data for markers ({info})...", false);
            }

            var startedReaderCount = 0;
            var markers = new SortedDictionary<string, int>(new OrdinalStringComparer());

            await source.AggregateParallelAsync(maxConcurrentCount, ct,
                isr =>
                {
                    startedReaderCount++;
                    Report($"starting reader #{isr.BlockOrder}, total started {startedReaderCount}");

                    var result = isr.SetStatistic(ct);

                    Report($"finished reader #{isr.BlockOrder}, size/lines/markers:{isr.BlockSize}/{isr.MaxLineCount}/{result.Count}");
                    return result;
                },
                result =>
                {
                    result.ForEach(m =>
                    {
                        if (!markers.ContainsKey(m.Key))
                            markers.Add(m.Key, m.Value);
                        else
                            markers[m.Key] += m.Value;
                    });
                    return markers.Keys.Count;
                });

            _logger?.Log($@"SetReaderStatistic: markers count={markers.Count}; repetitions min={markers.OrderBy(m => m.Value).FirstOrDefault()}, max={markers.OrderByDescending(m => m.Value).FirstOrDefault()}, total file lines={ReaderSquad.Sum(r => (long)r.MaxLineCount)}");
            _logger?.LogMemory();

            return markers;
        }

        private async Task ArrangeDataAsync(CancellationToken ct, List<Dictionary<string, int>> markerGroups, ObservableFileWriter ofw)
        {
            var index = -1;
            foreach (var mg in markerGroups)
            {
                ct.ThrowIfCancellationRequested();
                index++;

                var groupInfo = $"range: {mg.Keys.Min()}-{mg.Keys.Max()}";
                var info = $"{groupInfo}, count={mg.Keys.Count}, lines={mg.Values.Sum()}, groups={markerGroups.Count}";

                ProgressModel.UpdateStage($"Collecting data using marker group (#{index}: {info})...", false);
                ProgressModel.SetMain(markerGroups.Count, index, "Marker groups:");

                _logger?.Log($"{index}. Collect marker group lines {info}");

                using (var strg = new BlockingCollection<StringStructureInfo>())
                {
                    await ReaderSquad.AggregateParallelAsync<IndexedStreamReader, int, int>(ReaderSquad.Count,
                        ct,
                        isr =>
                        {
                            var result = isr.ReadUsingMarkers(mg.Keys, ssi => strg.Add(ssi, ct), true, ct);

                            _logger?.Log($"{index}. MarkerGroup {groupInfo} Reader #{isr.BlockOrder} done.");

                            return result;
                        }, null, null);

                    strg.CompleteAdding();
                    _logger?.Log($"{index}. Done {info}");

                    ct.ThrowIfCancellationRequested();

                    //stage 4 - sort and write
                    ProgressModel.UpdateStage($"Arrange and write data for marker group (#{index}: {info})...", false);
                    ProgressModel.SetDetail(strg.Count, 0, $"Writing arranged lines: ");

                    var isWritten = false;
                    var addedCounter = 0;

                    async void Report() => await Task.Run(() =>
                    {
                        do
                        {
                            ProgressModel.DetailValue = addedCounter.AdaptValue<T>();
                        } while (!isWritten);
                    }, ct);

                    _logger?.Log($"{index}. Sort & write marker group lines {info}");

                    Report();
                    foreach (var ssi in strg.GetConsumingEnumerable(ct).AsParallel().OrderBy(x => x))
                    {
                        ofw.AddItem(ssi.Text);
                        Interlocked.Increment(ref addedCounter);
                    }

                    isWritten = true;

                    _logger?.Log($"{index}. Finished {info}");
                }

                ProgressModel.Reset();
            }
        }

        public async Task<ulong> TransferSortedDataToFileAsync(ArrangeDataModel model, CancellationToken ct)
        {
            if (ProgressModel == null)
                throw new ArgumentException(nameof(ProgressModel));

            ulong result = 0;
            ulong totalLines = 0;
            ObservableFileWriter ofw = null;

            try
            {
                ProgressModel.DoRefresh = true;

                //stage 1 - split file on blocks
                ProgressModel.UpdateStage($"Splitting {new FileInfo(model.SourceFile).Name} by blocks={model.BlockSizeMb}MB", true);

                if (!InitReaders(model))
                    throw new Exception("Block readers initialization failed");

                //stage 2 - SetReaderStatistic
                ProgressModel.UpdateStage($"Analyzing readers data for markers...", true);

                var markers = await SetReaderStatisticAsync(ReaderSquad, model.MaxConcurrentCount, ct);
                if (markers.IsEmpty())
                    throw new Exception("Readers statistic failed, no markers found");

                totalLines = ReaderSquad.USum(r => (ulong)r.MaxLineCount);

                //stage 3 - arrange using marker groups
                ProgressModel.UpdateStage($"Collecting data using marker group...", true);

                var markerGroups = markers.GroupWhileAggregating(0,
                        (sum, item) => sum + item.Value,
                        (sum, item) => sum <= model.MaxMarkerGroupLineCount)
                    .Select(x => x.ToDictionary()).ToList();

                ofw = new ObservableFileWriter(model.TargetFile, 0, ct);
                await ArrangeDataAsync(ct, markerGroups, ofw);
            }
            catch (OperationCanceledException oce)
            {
                _logger?.LogException("TransferSortedDataToFileAsync terminated by user", oce);
            }
            catch (Exception e)
            {
                _logger?.LogException("Unexpected error in TransferSortedDataToFileAsync", e);
            }
            finally
            {
                ProgressModel.DoRefresh = false;
                result = ofw?.Complete() ?? 0;

                ReaderSquad.DisposeSequence();
            }

            _logger?.Log($"Completed {model.TargetFile} with {result} sorted && written lines of {totalLines} lines in {model.SourceFile}");
            _logger?.LogMemory();

            return result;
        }


    }
}
