using BLL.Comparer;
using BLL.Data;
using BLL.FileStream;
using BLL.Log;
using BLL.Processors;
using Core.Extensions;
using Core.Helper;
using Domain.Domain;
using Domain.Models;
using MoreLinq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Domain.Constants;

namespace Tester
{
    //25M~1GB, generation time ~40s
    class Program
    {
        private const long chunkSize = 1000000;
        private const uint fakeRowCount = (uint)(300 * chunkSize);
        private static string sourceFolder => @"C:\Source\TestFiles";

        private static string sourceFile = Path.Combine(sourceFolder, @"10mCon.txt");//test3B
        private static string targetFolder => @"X:\Source\TestFiles";
        private static string targetFile => Path.Combine(targetFolder, $@"test{DateTime.Now.ToString(DefaultDateTimeStampFormat)}.txt");

        private static void Log(string message) => Logger.Instance.Log(message);
        private static void LogMemory() => Logger.Instance.LogMemory();

        private static CancellationTokenSource cts = new CancellationTokenSource();

        static void Main(string[] args)
        {
            Log("Start");

            //TestFakeDataGeneration();

            TransferSortData();

            Console.ReadKey();
        }

        private static void TransferSortData()
        {
            LogMemory();

            var arrays = StopwatchHelper.GetWithStopwatch(() => new DataProcessor<long>(new MultiProgressBarModel<long>(), Logger.Instance)
                .TransferSortedDataToFileAsync(new ArrangeDataModel(sourceFile, targetFile, DefaultParallelConcurrentCount, DefaultReaderBlockSizeMB, DefaultReaderMarkerGroupLineCount/30), cts.Token).Result, "Transfer sorted data");
            Log(arrays.Description + "|" + arrays.Result);

            LogMemory();
        }

        private static void TestFakeDataGeneration()
        {
            LogMemory();

            var resGenAsync = StopwatchHelper.GetWithStopwatch(() => new FakeDataGenerator<long>(new MultiProgressBarModel<long>(), Logger.Instance).GenerateDataToFileAsync(new GenerateDataModel(targetFile, fakeRowCount, DefaultParallelConcurrentCount,512000,10,3), cts.Token).Result, "Generated test data rows async");
            Log(resGenAsync.Description);

            LogMemory();
        }

        private static void CustomTests()
        {
            var words = new List<string>() { "A", "bA", "a" };
            var _charMarkers = new SortedDictionary<string, int>(new OrdinalStringComparer());
            words.ForEach(l => _charMarkers.Add(l, l.Length));
            _charMarkers.Keys.ForEach(l => Console.Write(l + " ")); Console.WriteLine();
            _charMarkers.Keys.OrderBy(x => x).ForEach(l => Console.Write(l + " "));
            words.OrderBy(x => x).ForEach(l => Console.Write(l + " "));

            int num = 0;
            var writeTask = Task.Run(() => num++);
            for (int i = 0; i < 10; i++)
                writeTask = writeTask.ContinueWith((x) =>
                {
                    Task.Delay(x.Result % 2 == 0 ? 5000 : 10000).Wait();
                    Log("Finished " + x.Result);
                    return num++;
                });

            writeTask.Wait();

            var initial = GC.GetTotalMemory(true);
            var arr = new List<byte[]>();
            arr.AddRange(File.ReadLines(sourceFile).Select(x => Encoding.UTF8.GetBytes(x)));
            var ObjSize = (initial - GC.GetTotalMemory(false));
            Console.WriteLine($"{ObjSize} {ObjSize / arr.Count} {arr.Count}");
        }

        #region static operations
        public static bool IsFileSorted(string sourceFile)
        {
            string prevLine = null;
            var rownum = 0;
            var cmr = new NaturalNumberPointStringComparer();
            foreach (var line in File.ReadLines(sourceFile))
            {
                rownum++;
                if (cmr.Compare(prevLine ?? line, line) <= 0)
                    prevLine = line;
                else
                {
                    Log($"Unsorted rows: {rownum - 1} {prevLine} => {rownum} {line}");
                    return false;
                }
                if (rownum % 1000000 == 0)
                    Log($"Checked rows: {rownum}");
            }
            return true;
        }


        public static ulong TransferData(string sourceFile, string targetFile)
        {
            var ofw = new ObservableFileWriter(targetFile);
            foreach (var line in File.ReadLines(sourceFile, DefaultFileStreamEncoding))
            {
                ofw.AddItem(line);
            }
            return ofw.Complete();
        }

        public static int CopyData(string sourceFile, string targetFile)
        {
            var counter = 0;
            using (var sw = new StreamWriter(targetFile))
            using (var sr = new StreamReader(sourceFile))
            {
                while (!sr.EndOfStream)
                {
                    sw.WriteLine(sr.ReadLine());
                    counter++;
                }
            }
            return counter;
        }

        public static void CopyFile(string sourceFile, string targetFile)
        {
            File.Copy(sourceFile, targetFile);
        }
        #endregion static operations
    }
}
