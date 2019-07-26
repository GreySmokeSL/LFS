using System.Collections.Generic;
using Core.Extensions;
using Core.Helper;
using NM = BLL.Repository.WinAPIRepository;
using static Domain.Constants;

namespace BLL.Comparer
{
    public sealed class NaturalNumberPointStringComparer : IComparer<string>
    {
        private int Offset => LinePartDelimiter.Length;
        public string LinePartDelimiter { get; }

        public NaturalNumberPointStringComparer(string linePartDelimiter = DefaultLinePartDelimiter)
        {
            LinePartDelimiter = linePartDelimiter; 
        }

        public int Compare(string a, string b)
        {
            if (a == null || b == null)
                return string.CompareOrdinal(a, b);

            var aInd = a.IndexOf(LinePartDelimiter, SCO);
            var bInd = b.IndexOf(LinePartDelimiter, SCO);

            return aInd > -1 && bInd > -1
                ? Compare(a, aInd + Offset, b, bInd + Offset)
                : NM.StrCmpLogicalW(a, b);
        }

        /// <summary>
        /// Compares by string suffix first, then by numeric prefix
        /// </summary>
        /// <param name="strA">The first string to use in the comparison. </param>
        /// <param name="indexA">The starting index of the substring in <paramref name="strA" />. </param>
        /// <param name="strB">The second string to use in the comparison. </param>
        /// <param name="indexB">The starting index of the substring in <paramref name="strB" />. </param>
        /// <param name="maxStringLength">Maximum characters to compare</param>
        /// <returns>if substrings equals, result by numeric part of base strings</returns>
        public static int Compare(string strA, int indexA, string strB, int indexB, int maxStringLength = 1024)
        {
            return string.CompareOrdinal(strA, indexA, strB, indexB, maxStringLength)
                .ValueOrFunc(0, () => NM.StrCmpLogicalW(strA.Substring(0, indexA), strB.Substring(0, indexB)));
        }
    }
}
