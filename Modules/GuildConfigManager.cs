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
            Dictionary<ulong, GuildConfig> guildConfigs;

            string fileName = "guildConfigs.json";
            if (File.Exists(fileName))
            {
                using FileStream readStream = File.OpenRead(fileName);
                guildConfigs = await JsonSerializer.DeserializeAsync<Dictionary<ulong, GuildConfig>>(readStream);
                await readStream.DisposeAsync();

                if (guildConfigs == null) throw new  Exception("Failed to deserialize guildConfigs.json");

                if (guildConfigs.ContainsKey(guildID))
                {
                    guildConfigs[guildID] = guildConfig;
                }
                else
                {
                    guildConfigs.Add(guildID, guildConfig);
                }
            }
            else 
            {
                guildConfigs = new Dictionary<ulong, GuildConfig>
                {
                    { guildID, guildConfig }
                };
            }

            using FileStream createStream = File.Open(fileName, FileMode.Create);
            await JsonSerializer.SerializeAsync(createStream, guildConfigs);
            await createStream.DisposeAsync();
        }

        public static async Task<GuildConfig> ReadGuildConfigAsync(ulong guildID)
        {
            Dictionary<ulong, GuildConfig> guildConfigs;

            string fileName = "guildConfigs.json";
            if (File.Exists(fileName))
            {
                using FileStream openStream = File.OpenRead(fileName);
                guildConfigs = await JsonSerializer.DeserializeAsync<Dictionary<ulong, GuildConfig>>(openStream);
                await openStream.DisposeAsync();
                if (guildConfigs == null) throw new Exception("Failed to deserialize guildConfigs.json");

                if (guildConfigs.TryGetValue(guildID, out GuildConfig guildConfig))
                {
                    return guildConfig;
                }
                else throw new Exception("guildconfig not found");
            }
            else throw new Exception("guildConfig.json not found");
        }
    }
}
