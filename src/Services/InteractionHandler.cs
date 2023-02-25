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

            _client.InteractionCreated += HandleInteraction;
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                var context = new SocketInteractionContext(_client, arg);
                await _interactions.ExecuteCommandAsync(context, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
