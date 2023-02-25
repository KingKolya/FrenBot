using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text.Json;

namespace FrenBot.Modules
{
    public class Interactions : InteractionModuleBase<SocketInteractionContext>
    { 
        [SlashCommand("subscribe", "receive notification when users join a voice channel")]
        public async Task HandleSubscribeCommandAsync()
        {
            var user = Context.User as IGuildUser;
            if (user == null) return;

            var guildConfig = await ReadGuildInfoAsync();

            var role = Context.Guild.GetRole(guildConfig.NotifyRoleID);
            if (role == null)
            {
                await ReplyAsync($"Role not found.");
                return;
            }

            if (HasRole(guildConfig.NotifyRoleID))
            {
                await RespondAsync($"you are already subscribed", ephemeral: true);
                return;
            }
            else
            {
                await user.AddRoleAsync(role);
                await RespondAsync("you will now receive notifications when users join a voice channel.", ephemeral: true);
            }
        }

        [SlashCommand("unsubscribe", "stop receiving notifications when users join a voice channel")]
        public async Task HandleUnsubscribeCommandAsync()
        {
            var user = Context.User as IGuildUser;
            if (user == null) return;

            var guildConfig = await ReadGuildInfoAsync();

            var role = Context.Guild.GetRole(guildConfig.NotifyRoleID);
            if (role == null)
            {
                await ReplyAsync($"Role not found.");
                return;
            }

            if (HasRole(guildConfig.NotifyRoleID))
            {
                await RespondAsync("you will no longer receive notifications when users join a voice channel.", ephemeral: true);
                await user.RemoveRoleAsync(role);
            }
            else
            {               
                await RespondAsync("you are currently not subscribed.", ephemeral: true);
                return;
            }
        }

        [SlashCommand("enable", "Enable notifications")]
        public async Task HandleEnableCommandAsync()
        {
            var guildConfig = await ReadGuildInfoAsync();

            if (guildConfig.NotifyEnabled)
            {
                await RespondAsync("Notifications are already enabled.");
            }
            else
            {
                guildConfig.NotifyEnabled = true;

                await WriteGuildInfoAsync(guildConfig);
                await RespondAsync("Notifications are now enabled.");
            }

        }

        [SlashCommand("disable", "Disable notifications")]
        public async Task HandleDisableCommandAsync()
        {
            var guildConfig = await ReadGuildInfoAsync();

            if (guildConfig.NotifyEnabled)
            {
                guildConfig.NotifyEnabled = false;

                await WriteGuildInfoAsync(guildConfig);
                await RespondAsync("Notifications are now disabled.");
            }
            else
            {
                await RespondAsync("Notifications are currently not enabled");
            }
        }

        [SlashCommand("update", "update guild config")]
        public async Task HandleUpdateCommandAsync(string channel, string role)
        {
            ulong channelID = ulong.Parse(channel);
            ulong roleID = ulong.Parse(role);

            GuildConfig guildConfig = new()
            {
                NotifyChannelID = channelID,
                NotifyRoleID = roleID
            };

            await WriteGuildInfoAsync(guildConfig);
            await RespondAsync("channel and role have been update");
        }

        async Task WriteGuildInfoAsync(GuildConfig guildConfig)
        {
            string fileName = Context.Guild.Id.ToString() + ".json";

            using FileStream fileStream = File.Create(fileName);
            await JsonSerializer.SerializeAsync(fileStream, guildConfig);
            await fileStream.DisposeAsync();

            Console.WriteLine($"Updated file: {fileName} {Environment.NewLine} {File.ReadAllText(fileName)}");
        }
        async Task<GuildConfig> ReadGuildInfoAsync()
        {
            using FileStream openStream = File.OpenRead(Context.Guild.Id.ToString() + ".json");
            GuildConfig? guildInfo = await JsonSerializer.DeserializeAsync<GuildConfig>(openStream);
            await openStream.DisposeAsync();

            if (guildInfo != null)
            {
                return guildInfo;
            }
            else
            {
                throw new Exception("guildInfo not found");
            }
        }

        bool HasRole(ulong roleID)
        {
            var guild = (Context.Channel as SocketGuildChannel)?.Guild;
            var user = Context.User as IGuildUser;

            if (guild == null || user == null) return false;

            return user.RoleIds.Any(id => guild.GetRole(id).Id == roleID);
        }
    }
}
