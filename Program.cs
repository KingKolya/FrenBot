using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FrenBot.Modules;

namespace FrenBot
{
    class Program
    {
        public static Task Main() => new Program().MainAsync();

        private async Task MainAsync()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("config.json")
                .Build();

            IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) => services
                .AddSingleton(config)
                .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.All }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .AddSingleton<GuildConfigManager>()
                .AddSingleton<VoiceChatNotifier>())
                .Build();

            await RunAsync(host);
        }

        private async Task RunAsync(IHost host)
        {

            IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            var client = serviceProvider.GetRequiredService<DiscordSocketClient>();

            var manager = serviceProvider.GetRequiredService<GuildConfigManager>();
            await manager.InitializeAsync();

            var interactions = serviceProvider.GetRequiredService<InteractionService>();
            await serviceProvider.GetRequiredService<InteractionHandler>().InitializeAsync();

            var notifier = serviceProvider.GetRequiredService<VoiceChatNotifier>();
            await notifier.InitializeAsync();

            var config = serviceProvider.GetRequiredService<IConfigurationRoot>();

            client.Log += Log;
            interactions.Log += Log;

            client.Ready += async () =>
            {
                Console.WriteLine("Hello, World!");

                // gather guildids
                var guilds = client.Guilds;

                // register commands to servers
                foreach (var guild in guilds)
                {
                    ulong guildID = guild.Id;

                    await interactions.RegisterCommandsToGuildAsync(guildID);
                    Console.WriteLine($"registered commands to guildID: {guildID}");
                }
            };
            await client.LoginAsync(TokenType.Bot, config.GetSection("appConfig")["token"]);

            await client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            await Task.CompletedTask;
        }

    }
}