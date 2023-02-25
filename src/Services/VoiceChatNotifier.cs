using Discord.WebSocket;

namespace FrenBot.Services
{
    public class VoiceChatNotifier
    {
        private readonly DiscordSocketClient _client;

        public VoiceChatNotifier(DiscordSocketClient client)
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

                    GuildConfig guildConfig = await GuildConfigManager.ReadGuildConfigAsync(guild.Id);

                    if (guildConfig == null || !guildConfig.NotifyEnabled) return;

                    var notifyChannel = guild.GetTextChannel(guildConfig.NotifyChannelID);
                    if (notifyChannel == null)
                    {
                        Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss}: notification channel not found in {guild.Name}");
                        return;
                    }

                    var notifyRole = guild.GetRole(guildConfig.NotifyRoleID);
                    if (notifyRole == null)
                    {
                        Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss}: notification role not found in {guild.Name}");
                        return;
                    }

                    await notifyChannel.SendMessageAsync($"{notifyRole.Mention} {user.Username} has joined {vcName}.");
                }
            }
        }
    }
}
