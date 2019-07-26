using System;
using System.Text;

namespace Domain
{
    public static class Constants
    {
        public const string DefaultLinePartDelimiter = ". ";
        public const string DefaultWordDelimiter = " ";

        public static readonly string NL = Environment.NewLine;
        public static readonly Encoding DefaultFileStreamEncoding = Encoding.UTF8;
        public static readonly int DefaultParallelConcurrentCount = Environment.ProcessorCount;
        
        public const StringComparison SCO = StringComparison.Ordinal;

        public const ushort DefaultReaderBlockSizeMB = 1024;

        public const int KB = 1024;
        public const int MB = 0x100000;//1048576;
        public const int GB = 1073741824;
        public const int WriteFileStreamBufferSize = 0x10000;//65536=64KB;
        public const int ReadFileStreamBufferSize = MB;
        public const int StringCompareOrdinalMaxStringLength = 1024;
        public const int DefaultReaderMarkerGroupLineCount = 50_000_000;//for sort

        public const string DefaultDateFormat = "yyyyMMdd";
        public const string DefaultDateTimeStampFormat = "yyyyMMddHHmmssfff";
        public const string Digits = @"0123456789";
        public const string LatinLetters = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        public const string CyrillicLetters = @"АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзийклмнопрстуфхцчшщъыьэюя";

    }
}
