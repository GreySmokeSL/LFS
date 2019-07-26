using System.Runtime.InteropServices;
using System.Security;

namespace BLL.Repository
{
    [SuppressUnmanagedCodeSecurity]
    internal static class WinAPIRepository
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }
}