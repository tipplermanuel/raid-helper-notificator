using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaidNotificator.Application;
using RaidNotificator.Discord;
using RaidNotificator.Options;

namespace RaidNotificator.Hosting;

public sealed class DiscordBotHostedService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly BotOptions _options;
    private readonly SlashCommandRegistrar _registrar;
    private readonly SlashCommandHandler _slashHandler;
    private readonly RaidEventSyncService _sync;
    private readonly ILogger<DiscordBotHostedService> _logger;

    public DiscordBotHostedService(
        DiscordSocketClient client,
        BotOptions options,
        SlashCommandRegistrar registrar,
        SlashCommandHandler slashHandler,
        RaidEventSyncService sync,
        ILogger<DiscordBotHostedService> logger)
    {
        _client = client;
        _options = options;
        _registrar = registrar;
        _slashHandler = slashHandler;
        _sync = sync;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Log += OnDiscordLogAsync;
        _client.Ready += OnReadyAsync;
        _client.MessageReceived += _sync.HandleMessageReceivedAsync;
        _client.MessageUpdated += _sync.HandleMessageUpdatedAsync;
        _client.GuildScheduledEventCreated += _sync.HandleGuildScheduledEventCreatedAsync;
        _client.SlashCommandExecuted += _slashHandler.HandleAsync;

        await _client.LoginAsync(TokenType.Bot, _options.DiscordToken);
        await _client.StartAsync();

        _logger.LogInformation("Discord bot started.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _client.Log -= OnDiscordLogAsync;
        _client.Ready -= OnReadyAsync;
        _client.MessageReceived -= _sync.HandleMessageReceivedAsync;
        _client.MessageUpdated -= _sync.HandleMessageUpdatedAsync;
        _client.GuildScheduledEventCreated -= _sync.HandleGuildScheduledEventCreatedAsync;
        _client.SlashCommandExecuted -= _slashHandler.HandleAsync;

        await _client.LogoutAsync();
        await _client.StopAsync();

        _logger.LogInformation("Discord bot stopped.");
    }

    private async Task OnReadyAsync()
    {
        await _registrar.RegisterGuildCommandsAsync();
    }

    private Task OnDiscordLogAsync(LogMessage message)
    {
        _logger.LogInformation("[Discord] {Message}", message.Message);
        return Task.CompletedTask;
    }
}
