using System.Text.Json;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace FrenBot.Modules
{
    public class VoiceChatNotifier
    {
        private readonly DiscordSocketClient _client;

        public VoiceChatNotifier (DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task InitializeAsync()
        {
            _client.UserVoiceStateUpdated += OnUserVoiceStateUpdateAsync;

            await Task.CompletedTask;
        }

        // TODO: implement a method to prevent the bot from ping spamming

        private async Task OnUserVoiceStateUpdateAsync(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {

            if (oldState.VoiceChannel != newState.VoiceChannel)
            {
               
                var voiceChannels = _client.Guilds.SelectMany(guild => guild.VoiceChannels);
                var afkChannel = _client.Guilds.Select(guild => guild.AFKChannel).Where(channel => channel != null);

                if (voiceChannels.Any(channel => !user.IsBot && !afkChannel.Contains(newState.VoiceChannel) && channel.ConnectedUsers.Any()))
                {
                    var guild = newState.VoiceChannel.Guild;
                    var vcName = newState.VoiceChannel.Name;

                    using FileStream openStream = File.OpenRead(guild.Id.ToString() + ".json");
                    GuildConfig? guildConfig = await JsonSerializer.DeserializeAsync<GuildConfig>(openStream);
                    await openStream.DisposeAsync();

                    Console.WriteLine($"{user.Username} has joined {vcName} in {guild.Name}");

                    if (guildConfig == null || !guildConfig.NotifyEnabled) return;

                    var notifyChannel = guild.GetTextChannel(guildConfig.NotifyChannelID);
                    if (notifyChannel == null)
                    {
                        Console.WriteLine($"{guild.Id}: notification channel not found");
                        return;
                    }

                    var notifyRole = guild.GetRole(guildConfig.NotifyRoleID);
                    if (notifyRole == null)
                    {
                        Console.WriteLine($"{guild.Id}: notification role not found");
                        return;
                    }

                    await notifyChannel.SendMessageAsync($"{notifyRole.Mention} {user.Username} has joined {vcName}.");
                }
            }
        }
    }
}
