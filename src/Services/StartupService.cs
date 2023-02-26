using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FrenBot.Modules;
using Microsoft.Extensions.Configuration;
using System.Reflection;


namespace FrenBot.Services
{
    public class StartupService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly IConfigurationRoot _config;

        public StartupService(IServiceProvider provider, DiscordSocketClient client, InteractionService interactions, IConfigurationRoot config)
        {
            _provider = provider;
            _client = client;
            _interactions = interactions;
            _config = config;

            _client.Ready += OnReadyAsync;
        }

        public async Task StartUpAsync()
        {
            var token = _config.GetSection("appConfig")["token"];
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Please enter the bot's token into the 'config.json' file in the app's root directory.");

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnReadyAsync()
        {
            var guilds = _client.Guilds;

            foreach (var guild in guilds)
            {
                ulong guildID = guild.Id;

                await _interactions.RegisterCommandsToGuildAsync(guildID);
                Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss} [Info] FrenBot: registered commands to guildID: {guildID}");
            }
        }
    }
}
