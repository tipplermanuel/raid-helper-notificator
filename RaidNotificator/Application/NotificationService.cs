using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using RaidNotificator.Contracts;

namespace RaidNotificator.Application;

public sealed class NotificationService : INotificationService
{
    private readonly DiscordSocketClient _client;
    private readonly ISubscriberRepository _subscribers;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        DiscordSocketClient client,
        ISubscriberRepository subscribers,
        ILogger<NotificationService> logger)
    {
        _client = client;
        _subscribers = subscribers;
        _logger = logger;
    }

    public async Task NotifySubscribersAsync(ComponentBuilderV2 builder, CancellationToken cancellationToken = default)
    {
        var list = await _subscribers.GetAllAsync(cancellationToken);
        if (list.Count == 0)
        {
            _logger.LogInformation("No subscribers to notify.");
            return;
        }

        foreach (var sub in list)
        {
            try
            {
                var user = _client.GetUser(sub.Id);

                if (user == null)
                {
                    _logger.LogWarning("Subscriber {SubscriberId} not found.", sub.Id);
                    continue;
                }

                var dm = await user.CreateDMChannelAsync();
                await dm.SendMessageAsync(components: builder.Build());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to notify subscriber {SubscriberId}.", sub.Id);
            }
        }
    }
}