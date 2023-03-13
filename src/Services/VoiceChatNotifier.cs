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
        private async Task OnUserVoiceStateUpdateAsync(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var voiceChannel = newState.VoiceChannel;
            if (voiceChannel == oldState.VoiceChannel || voiceChannel == null || !voiceChannel.ConnectedUsers.Any() || user.IsBot) return;

            var guild = voiceChannel.Guild;
            if (voiceChannel == guild.AFKChannel) return;

            var guildConfig = await GuildConfigManager.GetGuildConfigAsync(guild.Id);
            if (guildConfig == null || !guildConfig.NotifyEnabled) return;

            var guildName = guild.Name;
            var vcName = voiceChannel.Name;

            var notifyChannel = guild.GetTextChannel(guildConfig.NotifyChannelID);
            if (notifyChannel == null)
            {
                Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss} [Warning] FrenBot: notification channel not found in {guildName}");
                return;
            }

            var notifyRole = guild.GetRole(guildConfig.NotifyRoleID);
            if (notifyRole == null)
            {
                Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss} [Warning] FrenBot: notification role not found in {guildName}");
                return;
            }

            Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss} [notification] FrenBot: {user.Username} joined {vcName} in {guildName}");
            await notifyChannel.SendMessageAsync($"{notifyRole.Mention} {guild.GetUser(user.Id).DisplayName} has joined {vcName}.");
        }
    }
}
