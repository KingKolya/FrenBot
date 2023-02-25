using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FrenBot.Modules;
using FrenBot.Services;

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
                .AddSingleton<StartupService>()
                .AddSingleton<LoggingService>()
                .AddSingleton<GuildConfigManager>()
                .AddSingleton<VoiceChatNotifier>())
                .Build();

            await RunAsync(host);
        }

        private async Task RunAsync(IHost host)
        {

            IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            serviceProvider.GetRequiredService<LoggingService>();
            serviceProvider.GetRequiredService<InteractionHandler>();
            serviceProvider.GetRequiredService<GuildConfigManager>();
            serviceProvider.GetRequiredService<VoiceChatNotifier>();

            await serviceProvider.GetRequiredService<StartupService>().StartUpAsync();
            await Task.Delay(-1);
        }
    }
}