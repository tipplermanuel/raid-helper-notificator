using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using RaidNotificator.Options;

namespace RaidNotificator.Discord;

public sealed class SlashCommandRegistrar
{
    private readonly DiscordSocketClient _client;
    private readonly BotOptions _options;
    private readonly ILogger<SlashCommandRegistrar> _logger;

    public SlashCommandRegistrar(
        DiscordSocketClient client,
        BotOptions options,
        ILogger<SlashCommandRegistrar> logger)
    {
        _client = client;
        _options = options;
        _logger = logger;
    }

    public async Task RegisterGuildCommandsAsync()
    {
        var guild = _client.GetGuild(_options.GuildId);
        if (guild == null)
        {
            _logger.LogWarning("Guild {GuildId} not found for command registration.", _options.GuildId);
            return;
        }

        var commands = new ApplicationCommandProperties[]
        {
            new SlashCommandBuilder()
                .WithName("init")
                .WithDescription("Initialisiert die DB mit den letzten 10 Raid-Helper Events.")
                .Build(),
            new SlashCommandBuilder()
                .WithName("reset")
                .WithDescription("Loescht alle Events aus der Datenbank.")
                .Build(),
            new SlashCommandBuilder()
                .WithName("sub")
                .WithDescription("Tragt dich in die Broadcast-Liste ein.")
                .Build(),
            new SlashCommandBuilder()
                .WithName("unsub")
                .WithDescription("Tragt dich aus der Broadcast-Liste aus.")
                .Build()
        };

        await guild.BulkOverwriteApplicationCommandAsync(commands);
        _logger.LogInformation("Guild commands registered for guild {GuildId}.", _options.GuildId);
    }
}