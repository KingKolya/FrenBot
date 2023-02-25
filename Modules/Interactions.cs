using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace FrenBot.Modules
{
    public class Interactions : InteractionModuleBase<SocketInteractionContext>
    {

        [SlashCommand("subscribe", "receive notification when users join a voice channel")]
        public async Task HandleSubscribeCommandAsync()
        {
            var user = Context.User as IGuildUser;
            if (user == null) return;

            var guildConfig = await GuildConfigManager.ReadGuildConfigAsync(Context.Guild.Id);

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

            var guildConfig = await GuildConfigManager.ReadGuildConfigAsync(Context.Guild.Id);

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
            var guildConfig = await GuildConfigManager.ReadGuildConfigAsync(Context.Guild.Id);

            if (guildConfig.NotifyEnabled)
            {
                await RespondAsync("Notifications are already enabled.");
            }
            else
            {
                guildConfig.NotifyEnabled = true;

                await GuildConfigManager.WriteGuildConfigAsync(Context.Guild.Id, guildConfig);
                await RespondAsync("Notifications are now enabled.");
            }

        }

        [SlashCommand("disable", "Disable notifications")]
        public async Task HandleDisableCommandAsync()
        {
            var guildConfig = await GuildConfigManager.ReadGuildConfigAsync(Context.Guild.Id);

            if (guildConfig.NotifyEnabled)
            {
                guildConfig.NotifyEnabled = false;

                await GuildConfigManager.WriteGuildConfigAsync(Context.Guild.Id, guildConfig);
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

            await GuildConfigManager.WriteGuildConfigAsync(Context.Guild.Id, guildConfig);
            await RespondAsync("channel and role have been update");
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
