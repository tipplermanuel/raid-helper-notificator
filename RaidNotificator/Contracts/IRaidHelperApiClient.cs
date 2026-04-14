using RaidNotificator.DTOs;

namespace RaidNotificator.Contracts;

public interface IRaidHelperApiClient
{
    Task<RaidEvent?> GetEventByMessageIdAsync(ulong messageId, CancellationToken cancellationToken = default);
}