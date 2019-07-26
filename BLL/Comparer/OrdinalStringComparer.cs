using System;
using System.Collections.Generic;

namespace BLL.Comparer
{
    public sealed class OrdinalStringComparer : IComparer<string>
    {
        public int Compare(string a, string b)
        {
            return string.Compare(a, b, StringComparison.Ordinal);
        }
    }

}
