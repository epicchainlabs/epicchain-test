using System;

namespace EpicChain.Collector
{
    public interface ILogger
    {
        void LogError(string text, Exception? exception = null);
        void LogWarning(string text);
    }
}