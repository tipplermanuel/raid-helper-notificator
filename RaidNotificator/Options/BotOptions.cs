namespace RaidNotificator.Options;

public sealed class BotOptions
{
    public string DiscordToken { get; set; } = string.Empty;
    public ulong GuildId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string RaidHelperBaseUrl { get; set; } = "https://raid-helper.xyz/api/v4/";
    public ulong AdminClientId { get; set; }
}