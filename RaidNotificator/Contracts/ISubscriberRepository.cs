using RaidNotificator.DTOs;

namespace RaidNotificator.Contracts;

public interface ISubscriberRepository
{
    Task<IReadOnlyCollection<Subscriber>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> AddIfMissingAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(ulong userId, CancellationToken cancellationToken = default);
}