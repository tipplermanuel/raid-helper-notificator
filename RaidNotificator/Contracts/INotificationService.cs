using Discord;

namespace RaidNotificator.Contracts;

public interface INotificationService
{
    Task NotifySubscribersAsync(ComponentBuilderV2 builder, CancellationToken cancellationToken = default);
}