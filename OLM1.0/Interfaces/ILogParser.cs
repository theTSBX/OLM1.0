using System.Collections.Generic;

namespace OutputLogManagerNEW.Interfaces
{
    public interface ILogParser
    {
        Dictionary<string, int> Parse(string logContent);
    }
}
