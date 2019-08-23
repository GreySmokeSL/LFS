using System;
using Domain.Enums;

namespace BLL.Log
{
    public interface ILogger
    {
        void Log(string message, bool addCurrentTime = true, LogTarget target = LogTarget.None);
        void LogMemory();
        void LogException(string message, Exception e);
    }
}
