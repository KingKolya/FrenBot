using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FrenBot.Services;

namespace FrenBot.Modules
{
    public class Interactions : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ping", "send ping request to bot")]
        public async Task HandlePingCommandAsync()
        {
            var latency = Context.Client.Latency;
            await RespondAsync($"Pong! {latency}ms");
        }

        // TODO: make sure role is below bot in role hierarchy
        // TODO: make sure user has role
        [SlashCommand("subscribe", "receive notification when users join a voice channel")]
        public async Task HandleSubscribeCommandAsync()
        {
            var guildConfig = await GuildConfigManager.GetGuildConfigAsync(Context.Guild.Id);
            var roleId = guildConfig.NotifyRoleID;
            var user = Context.Guild.GetUser(Context.User.Id);

            if (HasRole(user, roleId))
            {
                await RespondAsync($"you are already subscribed", ephemeral: true);
            }
            else
            {
                await user.AddRoleAsync(roleId);
                await RespondAsync("you will now receive notifications when users join a voice channel.", ephemeral: true);
            }
        }

        [SlashCommand("unsubscribe", "stop receiving notifications when users join a voice channel")]
        public async Task HandleUnsubscribeCommandAsync()
        {
            var guildConfig = await GuildConfigManager.GetGuildConfigAsync(Context.Guild.Id);
            var roleId = guildConfig.NotifyRoleID;
            var user = Context.Guild.GetUser(Context.User.Id);

            if (HasRole(user, roleId))
            {
                await RespondAsync("you will no longer receive notifications when users join a voice channel.", ephemeral: true);
                 await user.RemoveRoleAsync(roleId);
            }
            else
            {               
                await RespondAsync("you are currently not subscribed.", ephemeral: true);
            }
        }

        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [SlashCommand("enable", "Enable notifications")]
        public async Task HandleEnableCommandAsync()
        {
            var guildConfig = await GuildConfigManager.GetGuildConfigAsync(Context.Guild.Id);

            if (guildConfig.NotifyEnabled)
            {
                await RespondAsync("Notifications are already enabled.");
            }
            else
            {
                guildConfig.NotifyEnabled = true;

                await GuildConfigManager.AddGuildConfigAsync(Context.Guild.Id, guildConfig);
                await RespondAsync("Notifications are now enabled.");
            }

        }

        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [SlashCommand("disable", "Disable notifications")]
        public async Task HandleDisableCommandAsync()
        {
            var guildConfig = await GuildConfigManager.GetGuildConfigAsync(Context.Guild.Id);

            if (guildConfig.NotifyEnabled)
            {
                guildConfig.NotifyEnabled = false;

                await GuildConfigManager.AddGuildConfigAsync(Context.Guild.Id, guildConfig);
                await RespondAsync("Notifications are now disabled.");
            }
            else
            {
                await RespondAsync("Notifications are currently not enabled");
            }
        }

        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [SlashCommand("setrole", "Set which role will be notified")]
        public async Task HandleSetRoleCommandAsync(IRole role)
        {
            ulong roleID = role.Id;

            var guildConfig = await GuildConfigManager.GetGuildConfigAsync(Context.Guild.Id);
            guildConfig.NotifyRoleID = roleID;

            await GuildConfigManager.AddGuildConfigAsync(Context.Guild.Id, guildConfig);
            await RespondAsync($"{role.Name} will be notified");
        }

        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [SlashCommand("setchannel", "Set where notifications will be sent")]
        public async Task HandleSetChannelCommandAsync(IChannel channel)
        {
            ulong channelID = channel.Id;

            var guildConfig = await GuildConfigManager.GetGuildConfigAsync(Context.Guild.Id);
            guildConfig.NotifyChannelID = channelID;

            await GuildConfigManager.AddGuildConfigAsync(Context.Guild.Id, guildConfig);
            await RespondAsync($"notifications will be sent to {channel.Name}");
        }

        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [SlashCommand("setcooldown", "set time between notification per user in minutes")]
        public async Task HandleSetCooldownAsync(int minutes)
        {
            var guildconfig = await GuildConfigManager.GetGuildConfigAsync(Context.Guild.Id);
            guildconfig.CooldownTime = minutes;
            
            await GuildConfigManager.AddGuildConfigAsync(Context.Guild.Id , guildconfig);
            await RespondAsync($"there is now a {minutes} minute cooldown on notifications per user");
        }

        bool HasRole(SocketGuildUser user, ulong roleId)
        {
            var role = Context.Guild.GetRole(roleId) ?? throw new Exception("role not found");
            return user.Roles.Contains(role);
        }
    }
}
