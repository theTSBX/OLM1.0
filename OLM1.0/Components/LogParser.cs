using System.Collections.Generic;
using OutputLogManagerNEW.Interfaces;

namespace OutputLogManagerNEW.Components
{
    public class LogParser : ILogParser
    {
        public Dictionary<string, int> Parse(string logContent)
        {
            // Stubbed logic, replace with actual parsing
            return new Dictionary<string, int>
            {
                { "Version", 11 },
                { "VRAM_MB", 8024 },
                { "Driver", 6614 }
            };
        }
    }
}
