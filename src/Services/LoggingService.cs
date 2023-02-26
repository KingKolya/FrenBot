using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace FrenBot.Services
{
    public class LoggingService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;

        private string LogDirectory { get; }
        private string LogFile => Path.Combine(LogDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.txt");

        public LoggingService(DiscordSocketClient client, InteractionService interactions)
        {
            LogDirectory = Path.Combine(AppContext.BaseDirectory, "logs");

            _client = client;
            _interactions = interactions;

            _client.Log += OnLogAsync;
            _interactions.Log += OnLogAsync;
        }

        private Task OnLogAsync(LogMessage msg)
        {
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);
            if (!File.Exists(LogFile))
                File.Create(LogFile).Dispose();

            string logText = $"{DateTime.UtcNow:hh:mm:ss} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
            File.AppendAllText(LogFile, logText + "\n");

            return Console.Out.WriteLineAsync(logText);
        }
    }
}
