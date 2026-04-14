using RaidNotificator.DTOs;

namespace RaidNotificator.Contracts;

public interface IRegistrationDiffService
{
    RegistrationDiff? GetDiff(RaidEvent before, RaidEvent after);
}