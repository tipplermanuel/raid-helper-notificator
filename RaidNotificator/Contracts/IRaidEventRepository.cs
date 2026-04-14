using RaidNotificator.DTOs;

namespace RaidNotificator.Contracts;

public interface IRaidEventRepository
{
    Task UpsertAsync(RaidEvent raidEvent, CancellationToken cancellationToken = default);
    Task<RaidEvent?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task DeleteAllAsync(CancellationToken cancellationToken = default);
}