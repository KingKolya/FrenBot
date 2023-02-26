using Discord.WebSocket;
using Discord.Interactions;
using System.Reflection;

namespace FrenBot.Services
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;

        public InteractionHandler(DiscordSocketClient client, InteractionService interactions, IServiceProvider services)
        {
            _client = client;
            _interactions = interactions;
            _services = services;

            _client.InteractionCreated += OnInteractionCreated;
        }

        private async Task OnInteractionCreated(SocketInteraction arg)
        {
            if (arg.User.IsBot || arg.User.Id == _client.CurrentUser.Id) return;

            var context = new SocketInteractionContext(_client, arg);
            var result = await _interactions.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ToString());
        }
    }
}
