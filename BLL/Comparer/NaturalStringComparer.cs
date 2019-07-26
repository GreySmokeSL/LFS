using System.Collections.Generic;
using NM = BLL.Repository.WinAPIRepository;

namespace BLL.Comparer
{
    public sealed class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string a, string b)
        {
            return NM.StrCmpLogicalW(a, b);
        }
    }

}
