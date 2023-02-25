using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace FrenBot.Modules
{
    public class GuildConfigManager
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;

        public GuildConfigManager(DiscordSocketClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            _client.JoinedGuild += OnJoinedGuildAsync;
            _client.LeftGuild += OnLeftGuildAsync;
            await Task.CompletedTask;
        }

        private async Task OnJoinedGuildAsync(SocketGuild guild)
        {
            var config = _services.GetRequiredService<IConfigurationRoot>().GetSection("moduleConfig").GetSection("voicechatNotifier");
            string channelName = config["channelName"];
            string roleName = config["roleName"];

            var channel = await guild.CreateTextChannelAsync(channelName);
            Console.WriteLine($"created channel {channel.Id}");

            var role = await guild.CreateRoleAsync(roleName);
            Console.WriteLine($"created role {role.Id}");

            await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, OverwritePermissions.DenyAll(channel));
            await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.InheritAll);

            GuildConfig guildConfig = new()
            {
                NotifyChannelID = channel.Id,
                NotifyRoleID = role.Id 
            };

            await WriteGuildConfigAsync(guild.Id, guildConfig);
        }

        private async Task OnLeftGuildAsync(SocketGuild guild)
        {
            GuildConfig guildConfig = await ReadGuildConfigAsync(guild.Id);

            var channel = guild.GetChannel(guildConfig.NotifyChannelID);
            var role = guild.GetRole(guildConfig.NotifyRoleID);

            if (channel != null) await channel.DeleteAsync();
            if (role != null) await role.DeleteAsync();


        }

        public static async Task WriteGuildConfigAsync(ulong guildID, GuildConfig guildConfig)
        {
            string fileName = guildID.ToString() + ".json";
            
            using FileStream fileStream = File.Create(fileName);
            await JsonSerializer.SerializeAsync(fileStream, guildConfig);
            await fileStream.DisposeAsync();

            Console.WriteLine($"Added file: {fileName} {Environment.NewLine} {File.ReadAllText(fileName)}");
        }

        public static async Task<GuildConfig> ReadGuildConfigAsync(ulong guildID)
        {
            using FileStream openStream = File.OpenRead(guildID.ToString() + ".json");
            GuildConfig? guildConfig = await JsonSerializer.DeserializeAsync<GuildConfig>(openStream);
            await openStream.DisposeAsync();

            if (guildConfig != null)
            {
                return guildConfig;
            }
            else
            {
                throw new Exception("guildInfo not found");
            }
        }
    }
}
