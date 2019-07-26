using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BLL.FileStream;
using Core.Helper;
using BLL.Log;
using Bogus;
using Domain.Domain;
using Domain.Models;
using MoreLinq.Extensions;
using static Domain.Constants;
using BLL.Helper;
using Core.Extensions;

namespace BLL.Processors
{
    public class FakeDataGenerator<T>
    {
        public string LinePartDelimiter { get; }
        public MultiProgressBarModel<T> ProgressModel { get; }

        private readonly ILogger _logger;

        public FakeDataGenerator(MultiProgressBarModel<T> progressModel, ILogger logger = null, string linePartDelimiter = DefaultLinePartDelimiter)
        {
            _logger = logger;
            LinePartDelimiter = linePartDelimiter;
            ProgressModel = progressModel;
        }

        //Examples:
        //    1. Apple
        //    415. Apple
        //    2. Banana is yellow
        //    32. Cherry is the best

        private IEnumerable<string> GenerateStrings(uint maxRecordPerTask, byte maxRepetitionForText, byte maxSentenceWordCount, bool shuffle = true)
        {
            var faker = new Faker();
            var partTextCount = (uint)(maxRecordPerTask / Math.Ceiling((decimal)maxRepetitionForText / 2));
            var textVariants = EnumerableHelper.Repeat(partTextCount, f => faker.Random.Words(maxSentenceWordCount));
            var numbers = EnumerableHelper.Repeat(faker.Random.UShort(1, maxRepetitionForText), f => faker.Random.UInt(1, maxRecordPerTask / 10));

            var query = from text in textVariants
                        from num in numbers
                        select $"{num}{LinePartDelimiter}{text}";

            if (shuffle)
                query = query.Shuffle();

            foreach (var str in query) yield return str;
        }

        public async Task<ulong> GenerateDataToFileAsync(GenerateDataModel model, CancellationToken ct)
        {
            _logger?.Log($"Run GenerateDataToFileAsync with maxRecordCount={model.MaxRecordCount}, maxConcurrentCount={model.MaxConcurrentCount}, maxRecordPerTask={model.MaxRecordPerTask}, maxRepetitionForText={model.MaxRepetitionForText}, maxSentenceWordCount={model.MaxSentenceWordCount} to {model.TargetFile}");

            ulong result = 0;
            ObservableFileWriter ofw = null;

            try
            {
                var totalPortions = (ushort)Math.Ceiling(1.5 * model.MaxRecordCount / model.MaxRecordPerTask);

                ProgressModel.DoRefresh = true;

                ProgressModel.SetDetail<ushort>(totalPortions, 0, "Starting portion");
                ProgressModel.SetMain<uint>(model.MaxRecordCount, 0, "Written lines");

                ofw = new ObservableFileWriter(model.TargetFile, model.MaxRecordCount);
                await EnumerableHelper.Range(0, totalPortions)
                    .AggregateParallelAsync(model.MaxConcurrentCount, ct,
                        index =>
                        {
                            ProgressModel.DetailValue = index.AdaptValue<T>();

                            _logger?.Log($"Start portion #{index}/{totalPortions}");
                            return new Tuple<uint, IEnumerable<string>>(index,
                                GenerateStrings(model.MaxRecordPerTask, model.MaxRepetitionForText, model.MaxSentenceWordCount));
                        },
                        portion =>
                        {
                            foreach (var s in portion.Item2)
                                ofw.AddItem(s);

                            ProgressModel.MainValue = ofw.WrittenCounter.AdaptValue<T>();
                            _logger?.Log($"Finished lines portion #{portion.Item1}/{totalPortions}, total queued/written to file: {ofw.AddedCounter}/{ofw.WrittenCounter}");
                            return ofw.AddedCounter;
                        },
                        total => total >= model.MaxRecordCount);
            }
            catch (OperationCanceledException oce)
            {
                _logger?.LogException("GenerateDataToFileAsync terminated by user", oce);
            }
            catch (Exception e)
            {
                _logger?.LogException("Unexpected fail in GenerateDataToFileAsync", e);
            }
            finally
            {
                ProgressModel.DoRefresh = false;

                result = ofw?.Complete() ?? 0;
            }

            _logger?.Log($"Completed {model.TargetFile} with {result} lines of {model.MaxRecordCount}");

            return result;
        }
    }
}

