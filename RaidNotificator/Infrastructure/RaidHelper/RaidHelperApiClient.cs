using System.Net.Http.Json;
using RaidNotificator.Contracts;
using RaidNotificator.DTOs;

namespace RaidNotificator.Infrastructure.RaidHelper;

public sealed class RaidHelperApiClient : IRaidHelperApiClient
{
    private readonly HttpClient _http;

    public RaidHelperApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<RaidEvent?> GetEventByMessageIdAsync(ulong messageId, CancellationToken cancellationToken = default)
    {
        return await _http.GetFromJsonAsync<RaidEvent>($"events/{messageId}", cancellationToken);
    }
}