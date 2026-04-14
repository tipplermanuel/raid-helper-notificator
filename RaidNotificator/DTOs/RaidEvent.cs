using System.Text.Json.Serialization;

namespace RaidNotificator.DTOs;

public class RaidEvent
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("signUps")]
    public List<SignUp> SignUps { get; set; } = new();

    [JsonPropertyName("lastUpdated")]
    public long LastUpdated { get; set; }
}

public class SignUp
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("className")]
    public string ClassName { get; set; } = string.Empty;

    [JsonPropertyName("roleName")]
    public string RoleName { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("entryTime")]
    public long EntryTime { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("specName")]
    public string? SpecName { get; set; }

    [JsonPropertyName("spec2Name")]
    public string? Spec2Name { get; set; }
}