using System.Net.Http.Json;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaidNotificator.Components;

namespace RaidNotificator
{
    public class Bot : IBot
    {
        private Dictionary<ulong, RaidEvent> _events = new();
        
        private List<Subscriber>? _subscribers;
        
        private ServiceProvider? _serviceProvider;

        private readonly HttpClient _http = new();
        private readonly ILogger<Bot> _logger;
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        
        private RaidEvent? _pendingEvent;

        public Bot(
            ILogger<Bot> logger,
            IConfiguration configuration)
        {
            _subscribers = new List<Subscriber>
            {
                new Subscriber
                {
                    Id = 715055211865178213,
                }
            };
            _logger = logger;
            _configuration = configuration;

            DiscordSocketConfig config = new()
            {
                GatewayIntents = GatewayIntents.All
            };

            _client = new DiscordSocketClient(config);
            _commands = new CommandService();
        }

        public async Task StartAsync(ServiceProvider services)
        {
            string discordToken = _configuration["DCToken"] ?? throw new Exception("Missing Discord token");

            _client.Log += message =>
            {
                _logger.LogInformation(message.Message);
                return Task.CompletedTask;
            };
            
            _logger.LogInformation($"Starting up with token {discordToken}");

            _serviceProvider = services;

            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();

            _client.MessageReceived += HandleCommandAsync;
            _client.GuildScheduledEventCreated += HandleEventCreation;
            _client.GuildScheduledEventUpdated += HandleEventUpdate;
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("Shutting down");
            
            await _client.LogoutAsync();
            await _client.StopAsync();

        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg is not SocketUserMessage message)
            {
                return;
            }

            if (message.Author.Username.Contains("Raid-Helper"))
            {
                _logger.LogInformation(message.Type.ToString());
                await CollectAndStoreEventDataAsync(message.Id);
            }
        }

        private async Task CollectAndStoreEventDataAsync(ulong messageId)
        {
            _pendingEvent = await _http.GetFromJsonAsync<RaidEvent>($"https://raid-helper.xyz/api/v4/events/{messageId}");
        }

        private async Task HandleEventCreation(SocketGuildEvent newEvent)
        {
            if (_pendingEvent != null)
            {
                _events.Add(newEvent.Id, _pendingEvent);
            }
            _pendingEvent = null;
            
            await NotifySubscribers(InfoMessage.NewEventMessageAsync(newEvent).GetAwaiter().GetResult());
        }
        
        
        private async Task HandleEventUpdate(Cacheable<SocketGuildEvent,ulong> old, SocketGuildEvent after)
        {
            if (!_events.ContainsKey(after.Id))
            {
                return;
            }
            try
            {
                
                var updated =
                    await _http.GetFromJsonAsync<RaidEvent>(
                        $"https://raid-helper.xyz/api/v4/events/{_events[after.Id].Id}");

                if (updated == null)
                    return;
                
                var diff = DifferenceGenerator.GetRegistrationDiffAsync(_events[after.Id], updated).GetAwaiter()
                    .GetResult();

                _events[after.Id] = updated;

                if (diff != null)
                    await NotifySubscribers(InfoMessage.UpdateRegistrationInfoAsync(diff, updated).GetAwaiter().GetResult());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private async Task NotifySubscribers(ComponentBuilderV2 builder)
        {
            _subscribers?.ForEach(s => _client.GetUser(s.Id).CreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync(components: builder.Build()));
        }
    }
}