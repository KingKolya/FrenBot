using Discord.WebSocket;

namespace FrenBot.Services
{
    public class VoiceChatNotifier
    {
        private readonly DiscordSocketClient _client;

        public VoiceChatNotifier(DiscordSocketClient client)
        {
            _client = client;

            _client.UserVoiceStateUpdated += OnUserVoiceStateUpdateAsync;
        }

        // TODO: Add notification cooldown per user
        // TODO: check for optimizations of voicechannels, afkChannel, and if user joined statement.
        private async Task OnUserVoiceStateUpdateAsync(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            if (newState.VoiceChannel == null || oldState.VoiceChannel == newState.VoiceChannel) return;

            var voiceChannels = _client.Guilds.SelectMany(guild => guild.VoiceChannels);
            var afkChannel = _client.Guilds.Select(guild => guild.AFKChannel).Where(channel => channel != null);

            if (!voiceChannels.Any(channel => !user.IsBot && !afkChannel.Contains(newState.VoiceChannel) && channel.ConnectedUsers.Any())) return;

            var guild = newState.VoiceChannel.Guild;
            var vcName = newState.VoiceChannel.Name;

            GuildConfig guildConfig = await GuildConfigManager.GetGuildConfigAsync(guild.Id);

            if (guildConfig == null || !guildConfig.NotifyEnabled) return;

            var notifyChannel = guild.GetTextChannel(guildConfig.NotifyChannelID);
            if (notifyChannel == null)
            {
                Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss} [Warning] FrenBot: notification channel not found in {guild.Name}");
                return;
            }

            var notifyRole = guild.GetRole(guildConfig.NotifyRoleID);
            if (notifyRole == null)
            {
                Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss} [Warning] FrenBot: notification role not found in {guild.Name}");
                return;
            }

            Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss} [notification] FrenBot: {user.Username} joined {vcName} in {guild.Name}");
            await notifyChannel.SendMessageAsync($"{notifyRole.Mention} {guild.GetUser(user.Id).DisplayName} has joined {vcName}.");

        }
    }
}
