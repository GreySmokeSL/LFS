using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BLL.Data;
using Core.Extensions;
using Domain.Domain;
using Domain.Models;
using static Domain.Constants;

namespace BLL.FileStream
{
    public class IndexedStreamReader : StreamReader, IComparable<IndexedStreamReader>
    {
        public enum MarkerType
        {
            Symbol = 0,
            Word = 1
        }

        public string NewLineDelimiter
        {
            get => _newLineDelimiter;
            private set
            {
                _newLineDelimiter = value;
                NLDLength = GBC(_newLineDelimiter);
            }
        }

        public string LinePartDelimiter
        {
            get => _linePartDelimiter;
            private set
            {
                _linePartDelimiter = value;
                LPDLength = GBC(_linePartDelimiter);
            }
        }

        public ProgressInfo<long> CurrentProgress { get; } = new ProgressInfo<long>();
        public long BlockOffset { get; private set; }
        public int NLDLength { get; private set; }
        public byte LPDLength { get; private set; }
        public int MarkerLength { get; private set; }
        public MarkerType MarkerMode { get; private set; }
        public ushort BlockOrder { get; set; }
        public long BlockSize { get; set; }

        public int MaxLineCount
        {
            get => _isLineProcessed.Length;
            private set
            {
                _isLineProcessed.Length = value < 0 ? 0 : value;
                _isLineProcessed.SetAll(false);
            }
        }

        private readonly BitArray _isLineProcessed = new BitArray(0);
        private readonly List<ushort> _lineMarkerIndex = new List<ushort>();
        private readonly List<byte> _lineMarkerOffset = new List<byte>();
        private readonly IDictionary<ushort, string> _indexedMarkers = new Dictionary<ushort, string>();
        private string _linePartDelimiter;
        private string _newLineDelimiter;

        private string ReadLineSafe() => ReadLine() ?? string.Empty;
        public byte GBC(string s) => (byte)CurrentEncoding.GetByteCount(s);
        public byte[] GB(string s) => CurrentEncoding.GetBytes(s);
        public string GS(byte[] b) => CurrentEncoding.GetString(b);

        public IDictionary<string, int> SetStatistic(CancellationToken ct)
        {
            var foundSymbols = new Dictionary<string, KeyValueClass<ushort, int>>();
            ProcessLines(ct, (linenum, text) =>
            {
                var res = GetMarkerAfterLPD(text);
                var index = ushort.MaxValue;
                if (res.marker != null)
                {
                    if (!foundSymbols.ContainsKey(res.marker))
                    {
                        var orderNum = (ushort)foundSymbols.Count;
                        foundSymbols.Add(res.marker, new KeyValueClass<ushort, int>(orderNum, 0));
                        _indexedMarkers.Add(orderNum, res.marker);
                    }

                    foundSymbols[res.marker].Value++;
                    index = foundSymbols[res.marker].Key;
                }

                _lineMarkerIndex.Add(index);
                _lineMarkerOffset.Add(res.offset);
            });

            MaxLineCount = foundSymbols.Values.Sum(x => x.Value);
            return new SortedDictionary<string, int>(foundSymbols.ToDictionary(x => x.Key, x => x.Value.Value));
        }

        public int ReadUsingMarkers(IEnumerable<string> markers, Action<StringStructureInfo> processAction, bool markAsProcessed, CancellationToken ct)
        {
            var possibleMarkers = markers.Intersect(_indexedMarkers.Values).ToHashSet();

            return ProcessLines(ct, (linenum, text) =>
            {
                if (linenum >= MaxLineCount)
                    return;

                if (markAsProcessed && _isLineProcessed[linenum])
                    return;

                var mark = _indexedMarkers[_lineMarkerIndex[linenum]];
                if (!possibleMarkers.Contains(mark))
                    return;

                processAction.Invoke(new StringStructureInfo(_lineMarkerOffset[linenum], text));
                if (markAsProcessed)
                    _isLineProcessed.Set(linenum, true);
            });
        }

        private int ProcessLines(CancellationToken ct, Action<int, string> processAction)
        {
            if (this == Null || BlockSize == 0) return 0;
            if (string.IsNullOrEmpty(LinePartDelimiter))
                throw new ArgumentException($"Value of {nameof(LinePartDelimiter)} is empty");

            SetPosition(BlockOffset);
            CurrentProgress.Reset();
            CurrentProgress.Total = BlockSize;
            CurrentProgress.Description = "#" + BlockOrder;

            var linenum = -1;
            var delta = 0;
            while (!EndOfStream && delta < BlockSize)
            {
                ct.ThrowIfCancellationRequested();

                var buffer = ReadLineSafe();
                delta += buffer.Length + NLDLength;
                CurrentProgress.Completed = delta;

                processAction.Invoke(++linenum, buffer);
            }

            return linenum;
        }

        private (string marker, byte offset, byte length) GetMarkerAfterLPD(string buffer, MarkerType mode = MarkerType.Word)
        {
            var offset = buffer.IndexOf(LinePartDelimiter, SCO);
            if (offset > -1)
            {
                var index = (byte)(offset + LPDLength);
                switch (mode)
                {
                    case MarkerType.Symbol:
                        if (MarkerLength <= 0)
                            throw new ArgumentException($"Invalid value of {nameof(MarkerLength)} = {MarkerLength}");
                        if (index + MarkerLength - 1 < buffer.Length)
                            return (buffer.Substring(index, MarkerLength), index, (byte)MarkerLength);
                        break;
                    case MarkerType.Word:
                        var wordLength = buffer.IndexOf(DefaultWordDelimiter, index, SCO).ResultIfValue(-1, buffer.Length) - index;
                        var word = buffer.Substring(index, wordLength);
                        if (!string.IsNullOrWhiteSpace(word))
                            return (word, index, (byte)wordLength);
                        break;
                }
            }
            return (null, 0, 0);
        }

        public void SetPosition(long offset = 0)
        {
            DiscardBufferedData();
            BaseStream.Seek(offset, SeekOrigin.Begin);
        }

        //using bytecount
        public static SortedSet<IndexedStreamReader> SplitByBlocks(string sourceFile, long blockSize, string fileRowDelimiter, ProgressInfo<long> pi, MarkerType mode = MarkerType.Word, int markerLength = 0)
        {
            using (var isr = new IndexedStreamReader(sourceFile))
            {
                if (isr == Null)
                    return null;

                pi.Total = isr.BaseStream.Length;

                //to detect encoding
                isr.ReadLineSafe();
                long offset = isr.CurrentEncoding.GetPreamble().Length;
                isr.SetPosition();
                isr.LinePartDelimiter = fileRowDelimiter;
                isr.NewLineDelimiter = NL;
                isr.MarkerLength = markerLength;
                isr.MarkerMode = mode;

                var result = new SortedSet<IndexedStreamReader>();
                ushort isrNum = 0;

                while (!isr.EndOfStream)
                {
                    //var bisr = new IndexedStreamReader(isr.CurrentEncoding, sourceFile)
                    var bisr = new IndexedStreamReader(sourceFile, isr.CurrentEncoding, false, ReadFileStreamBufferSize)
                    {
                        BlockOffset = offset,
                        BlockOrder = isrNum++,
                        LinePartDelimiter = isr.LinePartDelimiter,
                        NewLineDelimiter = isr.NewLineDelimiter,
                        MarkerLength = isr.MarkerLength,
                        MarkerMode = isr.MarkerMode
                    };
                    result.Add(bisr);


                    if (offset + blockSize < isr.BaseStream.Length)
                    {
                        var delta = blockSize;
                        isr.SetPosition(offset + delta);
                        delta += isr.ReadLineSafe().Length + isr.NLDLength;
                        offset += delta;
                        bisr.BlockSize = delta;

                        pi.Description = $"Block #{bisr.BlockOrder}, size={bisr.BlockSize / MB}MB";
                        pi.Completed = offset;
                    }
                    else
                    {
                        bisr.BlockSize = (int)(isr.BaseStream.Length - offset);

                        pi.Description = $"Block #{bisr.BlockOrder}, size={bisr.BlockSize / MB}MB";
                        pi.Completed = isr.BaseStream.Length;
                        break;
                    }
                }
                return result;
            }
        }

        private string this[int delta] => ReadLine(BlockOffset + delta);
        private string ReadLine(long offset)
        {
            if (this == Null) return null;

            SetPosition(offset);
            return ReadLine();
        }

        public int CompareTo(IndexedStreamReader other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return BlockOrder.CompareTo(other.BlockOrder).ResultIfValue(0, BlockOffset.CompareTo(other.BlockOffset));
        }

        private IEnumerable<T> ReadBlockData<T>(Func<char[], T> prepareDataFunc, int bufferSize = ReadFileStreamBufferSize)
        {
            if (this == Null || BlockSize == 0) yield break;

            SetPosition(BlockOffset);

            if (bufferSize == 0)
                bufferSize = 0x1000;

            var readCount = 0;
            var samePool = ArrayPool<char>.Shared;
            var buffer = samePool.Rent(bufferSize);
            try
            {
                do
                {
                    readCount += ReadBlock(buffer, 0, buffer.Length);
                    yield return prepareDataFunc(buffer);
                } while (readCount < BlockSize);
            }
            finally
            {
                samePool.Return(buffer);
            }
        }

        #region Constructors

        public IndexedStreamReader(Encoding encoding, string path)
            : base(new BufferedStream(
                    new System.IO.FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, ReadFileStreamBufferSize, FileOptions.SequentialScan), ReadFileStreamBufferSize),
                encoding, false, ReadFileStreamBufferSize)
        {
        }
        public IndexedStreamReader(Stream stream) : base(stream) { }
        public IndexedStreamReader(Stream stream, bool detectEncodingFromByteOrderMarks) : base(stream, detectEncodingFromByteOrderMarks) { }
        public IndexedStreamReader(Stream stream, Encoding encoding) : base(stream, encoding) { }
        public IndexedStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks) : base(stream, encoding, detectEncodingFromByteOrderMarks) { }
        public IndexedStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize) : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize) { }
        public IndexedStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen) : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen) { }
        public IndexedStreamReader(string path) : base(path) { }
        public IndexedStreamReader(string path, bool detectEncodingFromByteOrderMarks) : base(path, detectEncodingFromByteOrderMarks) { }
        public IndexedStreamReader(string path, Encoding encoding) : base(path, encoding) { }
        public IndexedStreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks) : base(path, encoding, detectEncodingFromByteOrderMarks) { }
        public IndexedStreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize) : base(path, encoding, detectEncodingFromByteOrderMarks, bufferSize) { }
        #endregion Constructors
    }
}