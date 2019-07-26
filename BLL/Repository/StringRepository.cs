using System;

namespace BLL.Repository
{
    public static class StringRepository
    {
        //https://codeblog.jonskeet.uk/2011/04/05/of-memory-and-strings/
        //TargetFileSize ~100GB = 107374182400B
        //For average string length = 20B it has 1.5 billion rows (1 491 308 089)
        //25M rows ~ 1GB

        public static readonly int StringAllocationDelta = Environment.Is64BitProcess ? 26 : 14;
        public static readonly int StringRoundedSize = Environment.Is64BitProcess ? 8 : 4;

        public static int GetRealStringSize(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return 0;
            return (int)Math.Ceiling((double)(StringAllocationDelta + source.Length * 2) / StringRoundedSize) *
                   StringRoundedSize;
        }
    }
}
