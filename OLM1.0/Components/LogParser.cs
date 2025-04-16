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

            // UTC Offset
            var utcOffsetMatch = Regex.Match(content, @"INF Local UTC offset:\s*(.+)");
            result["utc_offset"] = utcOffsetMatch.Success ? utcOffsetMatch.Groups[1].Value.Trim() : "Unknown";

            // Version
            var versionMatch = Regex.Match(content, @"INF Version:\s+(.+?)\s+Compatibility");
            result["version"] = versionMatch.Success ? versionMatch.Groups[1].Value.Trim() : "Unknown";

            // System Info
            result["os"] = TryExtract(content, @"INF\s+OS:\s+(.+)");
            result["cpu"] = TryExtract(content, @"INF\s+CPU:\s+(.+)");
            result["ram"] = TryExtract(content, @"INF\s+RAM:\s+(.+)");

            // GPU
            string renderer = "Unknown", vram = "Unknown", driver = "Unknown";
            var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim().StartsWith("Direct3D:"))
                {
                    for (int j = i + 1; j < lines.Length; j++)
                    {
                        string line = lines[j].Trim();
                        if (string.IsNullOrWhiteSpace(line)) break;
                        if (line.StartsWith("Version:")) continue;
                        if (line.StartsWith("Renderer:")) renderer = line.Substring("Renderer:".Length).Trim();
                        else if (line.StartsWith("VRAM:")) vram = line.Substring("VRAM:".Length).Trim();
                        else if (line.StartsWith("Driver:")) driver = line.Substring("Driver:".Length).Trim();
                    }
                    break;
                }
            }
            result["gpu"] = renderer;
            result["vram"] = vram;
            result["driver"] = driver;

            // Mods
            var mods = Regex.Matches(content, @"INF \[MODS\].*Loaded Mod: (.+)", RegexOptions.Multiline)
                            .Cast<Match>()
                            .Select(m => m.Groups[1].Value.Trim())
                            .ToList();
            result["mods"] = mods;

            // Errors and Warnings
            result["warning_count"] = Regex.Matches(content, @"^.*WRN ", RegexOptions.Multiline).Count;
            result["error_count"] = Regex.Matches(content, @"^.*ERR ", RegexOptions.Multiline).Count;

            result["all_errors"] = Regex.Matches(content, @"^.*ERR .*$", RegexOptions.Multiline)
                                        .Cast<Match>()
                                        .Select(m => m.Value)
                                        .ToList();

            result["all_warnings"] = Regex.Matches(content, @"^.*WRN .*$", RegexOptions.Multiline)
                                          .Cast<Match>()
                                          .Select(m => m.Value)
                                          .ToList();

            // Players
            result["player_joins"] = ExtractPlayers(content, @"INF GMSG: Player '(.+?)' joined the game");
            result["player_leaves"] = ExtractPlayers(content, @"INF GMSG: Player '(.+?)' left the game");

            // Max concurrent players from join/leave
            var playerEvents = new List<(DateTime time, int delta)>();
            var playerLines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in playerLines)
            {
                var match = Regex.Match(line, @"^(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}).*INF GMSG: Player '(.+?)' (joined|left) the game");
                if (match.Success)
                {
                    var time = DateTime.Parse(match.Groups[1].Value);
                    var action = match.Groups[3].Value;
                    playerEvents.Add((time, action == "joined" ? 1 : -1));
                }
            }

            playerEvents = playerEvents.OrderBy(e => e.time).ToList();
            int current = 0, max = 0;
            foreach (var evt in playerEvents)
            {
                current += evt.delta;
                if (current > max) max = current;
            }
            result["max_players"] = max;

            // Exceptions
            var allExceptions = ExtractExceptions(content);
            result["all_exceptions"] = allExceptions;
            result["exc_block"] = allExceptions.FirstOrDefault() ?? "";

            return result;
        }

        private static List<string> ExtractExceptions(string content)
        {
            var blocks = new List<string>();
            var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                bool isExceptionLine = line.Contains(" EXC ") || Regex.IsMatch(line, @"^\s*\w*Exception:");
                bool nextLineIsException = (i + 1 < lines.Length) && Regex.IsMatch(lines[i + 1], @"^\s*\w*Exception:");

                if (isExceptionLine || nextLineIsException)
                {
                    var block = new List<string>();
                    if (!Regex.IsMatch(line, @"^\s*\w*Exception:"))
                        block.Add(line);

                    if (i + 1 < lines.Length)
                    {
                        block.Add(lines[i + 1]);
                        i++;
                    }

                    for (int j = i + 1; j < lines.Length; j++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[j])) break;
                        block.Add(lines[j]);
                    }

                    blocks.Add(string.Join("\n", block));
                }
            }

            return blocks;
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
