using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace FrenBot.Services
{
    public class LoggingService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;

        private string _logDirectory { get; }
        private string _logFile => Path.Combine(_logDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.txt");

        public LoggingService(DiscordSocketClient client, InteractionService interactions)
        {
            _logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");

            _client = client;
            _interactions = interactions;

            _client.Log += OnLogAsync;
            _interactions.Log += OnLogAsync;
        }

        private Task OnLogAsync(LogMessage msg)
        {
            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
            if (!File.Exists(_logFile))
                File.Create(_logFile).Dispose();

            string logText = $"{DateTime.UtcNow:hh:mm:ss} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
            File.AppendAllText(_logFile, logText + "\n");

            return Console.Out.WriteLineAsync(logText);
        }
    }
}
