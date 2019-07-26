using System;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using static Domain.Constants;

namespace BLL.Helper
{
    public static class SystemResourcesHelper
    {
        static SystemResourcesHelper() { }

        private static readonly Lazy<ComputerInfo> Lazy = new Lazy<ComputerInfo>(() => new ComputerInfo());
        public static ComputerInfo Instance => Lazy.Value;

        public static string GetMemoryInfo()
        {
            return $"Memory free, MB: physical => {Instance.AvailablePhysicalMemory / MB} of {Instance.TotalPhysicalMemory / MB}, virtual => {Instance.AvailableVirtualMemory / MB} of {Instance.TotalVirtualMemory / MB} ";
        }

        public static string GetAppUsedMemoryInfo()
        {
            return $"Memory used, MB: GC => {GC.GetTotalMemory(false) / MB}, ProcPrivate => {Process.GetCurrentProcess().PrivateMemorySize64 / MB}";
        }
    }
}
