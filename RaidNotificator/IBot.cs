using Microsoft.Extensions.DependencyInjection;

namespace RaidNotificator
{
    public interface IBot
    {
        Task StartAsync(ServiceProvider services);

        Task StopAsync();
    }
}