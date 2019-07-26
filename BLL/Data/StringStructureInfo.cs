using System;
using BLL.Repository;
using Domain;

namespace BLL.Data
{
    [Serializable]
    public class StringStructureInfo : IComparable<StringStructureInfo>
    {
        public StringStructureInfo(byte substringIndex, string text)
        {
            SubstringIndex = substringIndex;
            Text = text;
        }

        public byte SubstringIndex { get; }
        public string Text { get; }

        //Examples - Number. String :
        //    1. Apple
        //    415. Apple
        //    2. Banana is yellow
        //    32. Cherry is the best
        public int CompareTo(StringStructureInfo other)
        {
            var result = string.CompareOrdinal(Text, SubstringIndex, other.Text, other.SubstringIndex, Constants.StringCompareOrdinalMaxStringLength);
            return result == 0 ? WinAPIRepository.StrCmpLogicalW(Text.Substring(0, SubstringIndex), other.Text.Substring(0, other.SubstringIndex)) : result;
        }
    }
}

