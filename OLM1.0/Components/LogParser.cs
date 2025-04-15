using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OutputLogManagerNEW.Components
{
    public class LogParser
    {
        public Dictionary<string, object> Parse(string content)
        {
            var result = new Dictionary<string, object>();

            // Duration (based on timestamps)
            var timestamps = Regex.Matches(content, @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}", RegexOptions.Multiline)
                                   .Select(m => DateTime.Parse(m.Value))
                                   .OrderBy(dt => dt)
                                   .ToList();
            result["log_duration"] = timestamps.Count >= 2 ? timestamps.Last() - timestamps.First() : TimeSpan.Zero;

            // Version line
            var versionMatch = Regex.Match(content, @"INF Version:\s+(.+?)\s+Compatibility");
            result["version"] = versionMatch.Success ? versionMatch.Groups[1].Value.Trim() : "Unknown";

            // System Info block
            result["os"] = TryExtract(content, @"INF\s+OS:\s+(.+)");
            result["cpu"] = TryExtract(content, @"INF\s+CPU:\s+(.+)");
            result["ram"] = TryExtract(content, @"INF\s+RAM:\s+(.+)");

            // GPU (from Direct3D block)
            var d3dMatch = Regex.Match(content, @"^Direct3D:\s*((?:\s{4}.+\n?)+)", RegexOptions.Multiline);
            string renderer = "Unknown", vram = "Unknown", driver = "Unknown";

            if (d3dMatch.Success)
            {
                var lines = d3dMatch.Groups[1].Value.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                var gpuLines = new List<string>();
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("Version:")) continue;
                    gpuLines.Add(trimmed);

                    if (trimmed.StartsWith("Renderer:")) renderer = trimmed.Substring("Renderer:".Length).Trim();
                    if (trimmed.StartsWith("VRAM:")) vram = trimmed.Substring("VRAM:".Length).Trim();
                    if (trimmed.StartsWith("Driver:")) driver = trimmed.Substring("Driver:".Length).Trim();
                }
                result["gpu"] = string.Join("\n", gpuLines);
            }
            else
            {
                result["gpu"] = "Unknown";
            }

            result["renderer"] = renderer;
            result["vram"] = vram;
            result["driver"] = driver;

            // Mods
            var mods = Regex.Matches(content, @"INF \[MODS\].*Loaded Mod: (.+)", RegexOptions.Multiline)
                            .Cast<Match>()
                            .Select(m => m.Groups[1].Value.Trim())
                            .ToList();
            result["mods"] = mods;

            // Warnings / Errors
            result["warning_count"] = Regex.Matches(content, @"^.*WRN ", RegexOptions.Multiline).Count;
            result["error_count"] = Regex.Matches(content, @"^.*ERR ", RegexOptions.Multiline).Count;

            // Players
            result["player_joins"] = ExtractPlayers(content, @"INF GMSG: Player '(.+?)' joined the game");
            result["player_leaves"] = ExtractPlayers(content, @"INF GMSG: Player '(.+?)' left the game");

            return result;
        }

        private static string TryExtract(string content, string pattern)
        {
            var match = Regex.Match(content, pattern, RegexOptions.Multiline);
            return match.Success ? match.Groups[1].Value.Trim() : "Unknown";
        }

        private static List<string> ExtractPlayers(string content, string pattern)
        {
            return Regex.Matches(content, pattern, RegexOptions.Multiline)
                        .Cast<Match>()
                        .Select(m => m.Groups[1].Value.Trim())
                        .Distinct()
                        .ToList();
        }
    }
}
