using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using RaidNotificator.Components;
using RaidNotificator.Contracts;
using RaidNotificator.Options;

namespace RaidNotificator.Application;

public sealed class RaidEventSyncService
{
    private readonly DiscordSocketClient _client;
    private readonly BotOptions _options;
    private readonly IRaidHelperApiClient _api;
    private readonly IRaidEventRepository _events;
    private readonly IRegistrationDiffService _diffs;
    private readonly INotificationService _notifications;
    private readonly ILogger<RaidEventSyncService> _logger;

    public RaidEventSyncService(
        DiscordSocketClient client,
        BotOptions options,
        IRaidHelperApiClient api,
        IRaidEventRepository events,
        IRegistrationDiffService diffs,
        INotificationService notifications,
        ILogger<RaidEventSyncService> logger)
    {
        _client = client;
        _options = options;
        _api = api;
        _events = events;
        _diffs = diffs;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task<int> InitializeFromConfiguredChannelAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var channel = GetConfiguredTextChannel();
        if (channel == null)
        {
            _logger.LogWarning("Configured channel not found.");
            return 0;
        }

        var messages = await channel.GetMessagesAsync(take).FlattenAsync();
        var count = 0;

        foreach (var msg in messages)
        {
            var evt = await _api.GetEventByMessageIdAsync(msg.Id, cancellationToken);
            if (evt == null)
            {
                continue;
            }

            await _events.UpsertAsync(evt, cancellationToken);
            count++;
        }

        return count;
    }

    public async Task HandleMessageReceivedAsync(SocketMessage arg)
    {
        if (arg is not SocketUserMessage message)
        {
            return;
        }

        if (!message.Author.Username.Contains("Raid-Helper", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var raidEvent = await _api.GetEventByMessageIdAsync(message.Id);
        if (raidEvent == null)
        {
            _logger.LogInformation("Raid-Helper API has no event for message {MessageId}.", message.Id);
            return;
        }

        await _events.UpsertAsync(raidEvent);
    }

    public async Task HandleMessageUpdatedAsync(
        Cacheable<IMessage, ulong> before,
        SocketMessage after,
        ISocketMessageChannel channel)
    {
        var previous = await _events.GetByIdAsync(after.Id.ToString());
        if (previous == null)
        {
            return;
        }

        var updated = await _api.GetEventByMessageIdAsync(after.Id);
        if (updated == null)
        {
            return;
        }

        var diff = _diffs.GetDiff(previous, updated);
        if (diff == null)
        {
            return;
        }

        await _events.UpsertAsync(updated);
        var message = InfoMessage.UpdateRegistrationInfoAsync(diff, updated);
        await _notifications.NotifySubscribersAsync(message);
    }

    public async Task HandleGuildScheduledEventCreatedAsync(SocketGuildEvent guildEvent)
    {
        var message = InfoMessage.NewEventMessageAsync(guildEvent);
        await _notifications.NotifySubscribersAsync(message);
    }

    private SocketTextChannel? GetConfiguredTextChannel()
    {
        var guild = _client.GetGuild(_options.GuildId);
        return guild?.TextChannels.FirstOrDefault(c =>
            string.Equals(c.Name, _options.ChannelName, StringComparison.OrdinalIgnoreCase));
    }
}
