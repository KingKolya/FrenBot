using Discord.Interactions;
using Discord.WebSocket;
using System.Text.Json;

namespace FrenBot.Services
{
    public class GuildConfigManager
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private const string FileName = "guildConfigs.json";
        public GuildConfigManager(DiscordSocketClient client, InteractionService interactions)
        {
            _client = client;
            _interactions = interactions;

            _client.JoinedGuild += OnJoinedGuildAsync;
            _client.LeftGuild += OnLeftGuildAsync;
        }

        private async Task OnJoinedGuildAsync(SocketGuild guild)
        {
            await _interactions.RegisterCommandsToGuildAsync(guild.Id);
            Console.WriteLine($"registered commands to guildID: {guild.Id}");

            GuildConfig guildConfig = new();
            await AddGuildConfigAsync(guild.Id, guildConfig);
        }

        private async Task OnLeftGuildAsync(SocketGuild guild)
        {
            await RemoveGuildConfigAsync(guild.Id);
        }

        public static async Task<GuildConfig> GetGuildConfigAsync(ulong guildId)
        {
            var guildConfigs = await ReadGuildConfigsAsync();

            if (!guildConfigs.ContainsKey(guildId))
                throw new Exception($"guildconfig {guildId} not found");

            return guildConfigs[guildId];
        }

        // TODO: append file instead of rewriting it
        public static async Task AddGuildConfigAsync(ulong guildId, GuildConfig guildConfig)
        {
            Dictionary<ulong, GuildConfig> _guildConfigs;
            if (File.Exists(FileName))
            {
                var guildConfigs = await ReadGuildConfigsAsync();

                if (guildConfigs.ContainsKey(guildId))
                {
                    guildConfigs[guildId] = guildConfig;
                    Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss} [Info] FrenBot: Updated guildConfig {guildId};");
                }
                else
                {
                    guildConfigs.Add(guildId, guildConfig);
                    Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss} [Info] FrenBot: Added guildConfig {guildId};");
                }
                _guildConfigs = guildConfigs;
            }
            else
            {
                var guildConfigs = new Dictionary<ulong, GuildConfig>
                {
                    { guildId, guildConfig }
                };
                _guildConfigs = guildConfigs;
            }

            await WriteGuildConfigsAsync(_guildConfigs);
        }

        private static async Task RemoveGuildConfigAsync(ulong guildId)
        {
            var guildConfigs = await ReadGuildConfigsAsync();

            if (!guildConfigs.ContainsKey(guildId)) return;
            guildConfigs.Remove(guildId);
            Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss} [Info] FrenBot: Removed guildConfig {guildId};");

            await WriteGuildConfigsAsync(guildConfigs);
        }

        private static async Task<Dictionary<ulong, GuildConfig>> ReadGuildConfigsAsync()
        {
            if (!File.Exists(FileName)) throw new Exception($"{FileName} not found");

            FileStream openStream = File.OpenRead(FileName);
            var guildConfigs = await JsonSerializer.DeserializeAsync<Dictionary<ulong, GuildConfig>>(openStream);
            await openStream.DisposeAsync();

            if (guildConfigs == null) throw new Exception($"Failed to deserialize {FileName}");
            return guildConfigs;
        }

        private static async Task WriteGuildConfigsAsync(Dictionary<ulong, GuildConfig> guildConfigs)
        {
            FileStream createStream = File.Create(FileName);
            await JsonSerializer.SerializeAsync(createStream, guildConfigs);
            await createStream.DisposeAsync();
        }
    }
}
