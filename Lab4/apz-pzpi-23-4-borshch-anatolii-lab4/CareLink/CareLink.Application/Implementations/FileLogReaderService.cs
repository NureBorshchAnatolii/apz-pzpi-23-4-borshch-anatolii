using System.Text.RegularExpressions;
using CareLink.Application.Dtos.Admin;
using Microsoft.Extensions.Configuration;

namespace CareLink.Application.Implementations
{
    public class FileLogReaderService
    {
        private readonly string _logFolderPath;

        public FileLogReaderService(IConfiguration config)
        {
            _logFolderPath = Path.GetDirectoryName(config["Logging:FilePath"] ?? "logs/log-.txt") ?? "logs";
        }

        public async Task<IEnumerable<SystemLogDto>> ReadLogsAsync(
            DateTime? from = null,
            DateTime? to = null,
            string? level = null)
        {
            if (!Directory.Exists(_logFolderPath))
                return Enumerable.Empty<SystemLogDto>();

            var logFiles = Directory.GetFiles(_logFolderPath, "log-*.txt");
            var logs = new List<SystemLogDto>();

            foreach (var file in logFiles)
            {
                using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);

                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var log = ParseLine(line);
                    if (log != null)
                        logs.Add(log);
                }
            }

            if (from.HasValue) logs = logs.Where(l => l.Timestamp >= from.Value).ToList();
            if (to.HasValue) logs = logs.Where(l => l.Timestamp <= to.Value).ToList();
            if (!string.IsNullOrEmpty(level))
                logs = logs.Where(l => l.Level.Equals(level, StringComparison.OrdinalIgnoreCase)).ToList();

            return logs;
        }

        private SystemLogDto? ParseLine(string line)
        {
            var match = Regex.Match(line,
                @"^(?<date>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}) [+-]\d{2}:\d{2} \[(?<level>\w+)\] (?<message>.+)$");

            if (!match.Success) return null;

            return new SystemLogDto
            {
                Timestamp = DateTime.Parse(match.Groups["date"].Value),
                Level = match.Groups["level"].Value,
                Source = "System",
                Message = match.Groups["message"].Value
            };
        }
    }
}