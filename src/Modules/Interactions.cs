﻿using Discord;
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
            await ReplyAsync($"Pong! {latency}ms");
        }

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
            }
        }

        [DefaultMemberPermissions(GuildPermission.Administrator)]
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

        [DefaultMemberPermissions(GuildPermission.Administrator)]
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

        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [SlashCommand("setrole", "Set which role will be notified")]
        public async Task HandleSetRoleCommandAsync(IRole role)
        {
            ulong roleID = role.Id;

            var guildConfig = await GuildConfigManager.ReadGuildConfigAsync(Context.Guild.Id);
            guildConfig.NotifyRoleID = roleID;

            await GuildConfigManager.WriteGuildConfigAsync(Context.Guild.Id, guildConfig);
            await RespondAsync($"{role.Name} will be notified");
        }

        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [SlashCommand("setchannel", "Set where notifications will be sent")]
        public async Task HandleSetChannelCommandAsync(IChannel channel)
        {
            ulong channelID = channel.Id;

            var guildConfig = await GuildConfigManager.ReadGuildConfigAsync(Context.Guild.Id);
            guildConfig.NotifyChannelID = channelID;

            await GuildConfigManager.WriteGuildConfigAsync(Context.Guild.Id, guildConfig);
            await RespondAsync($"notifications will be sent to {channel.Name}");
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