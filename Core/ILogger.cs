using System.Collections.Generic;

namespace Core
{
    public interface ILogger
    {
        IReadOnlyList<string> Logs { get; }

        void Log(string format, params object[] args);
    }
}
