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
            Console.WriteLine($"{DateTime.Now}: bot has joined {guild.Name}");

            var config = _services.GetRequiredService<IConfigurationRoot>().GetSection("moduleConfig").GetSection("voicechatNotifier");
            var channelName = config["channelName"];
            var roleName = config["roleName"];

            var channel = await guild.CreateTextChannelAsync(channelName);
            Console.WriteLine($"{DateTime.Now}: created channel {channel.Id} in {guild.Name}");

            var role = await guild.CreateRoleAsync(roleName);
            Console.WriteLine($"{DateTime.Now}: created role {role.Id} in {guild.Name}");

            await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.InheritAll);
            await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, OverwritePermissions.DenyAll(channel));

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
            string fileName = "guildConfigs.json";

            Dictionary<ulong, GuildConfig> _guildConfigs;
            if (File.Exists(fileName))
            {
                FileStream readStream = File.OpenRead(fileName);
                var guildConfigs = await JsonSerializer.DeserializeAsync<Dictionary<ulong, GuildConfig>>(readStream);
                await readStream.DisposeAsync();

                if (guildConfigs == null) throw new  Exception("Failed to deserialize guildConfigs.json");

                if (guildConfigs.ContainsKey(guildID))
                {
                    guildConfigs[guildID] = guildConfig;
                    Console.WriteLine($"{DateTime.Now}: Updated guildConfig {guildID};");
                }
                else
                {
                    guildConfigs.Add(guildID, guildConfig);
                    Console.WriteLine($"{DateTime.Now}: Added guildConfig {guildID};");
                }
                _guildConfigs = guildConfigs;
            }
            else 
            {
                var guildConfigs = new Dictionary<ulong, GuildConfig>
                {
                    { guildID, guildConfig }
                };
                _guildConfigs = guildConfigs;
            }

            FileStream createStream = File.Open(fileName, FileMode.Create);
            await JsonSerializer.SerializeAsync(createStream, _guildConfigs);
            await createStream.DisposeAsync();
        }

        public static async Task<GuildConfig> ReadGuildConfigAsync(ulong guildID)
        {
            string fileName = "guildConfigs.json";
            if (File.Exists(fileName))
            {
                FileStream openStream = File.OpenRead(fileName);
                var guildConfigs = await JsonSerializer.DeserializeAsync<Dictionary<ulong, GuildConfig>>(openStream);
                await openStream.DisposeAsync();
                if (guildConfigs == null) throw new Exception("Failed to deserialize guildConfigs.json");

                if (guildConfigs.ContainsKey(guildID))
                {
                    return guildConfigs[guildID];
                }
                else throw new Exception($"guildconfig {guildID} not found");
            }
            else throw new Exception("guildConfig.json not found");
        }
    }
}
