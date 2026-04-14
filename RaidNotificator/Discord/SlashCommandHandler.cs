using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using RaidNotificator.Application;
using RaidNotificator.Contracts;
using RaidNotificator.Options;

namespace RaidNotificator.Discord;

public sealed class SlashCommandHandler
{
    private readonly RaidEventSyncService _sync;
    private readonly IRaidEventRepository _events;
    private readonly ISubscriberRepository _subs;
    private readonly BotOptions _options;
    private readonly ILogger<SlashCommandHandler> _logger;

    public SlashCommandHandler(
        RaidEventSyncService sync,
        IRaidEventRepository events,
        ISubscriberRepository subs,
        BotOptions options,
        ILogger<SlashCommandHandler> logger)
    {
        _sync = sync;
        _events = events;
        _subs = subs;
        _logger = logger;
        _options = options;
    }

    public async Task HandleAsync(SocketSlashCommand command)
    {
        try
        {
            switch (command.CommandName)
            {
                case "init":
                    if (!IsAdmin(command.User.Id))
                    {
                        await RespondAsync(command, "Du hast keine Berechtigung für diesen Command.");
                        return;
                    }
                    var count = await _sync.InitializeFromConfiguredChannelAsync(10);
                    await RespondAsync(command, $"Init abgeschlossen. {count} Events verarbeitet.");
                    break;

                case "reset":
                    if (!IsAdmin(command.User.Id))
                    {
                        await RespondAsync(command, "Du hast keine Berechtigung für diesen Command.");
                        return;
                    }
                    await _events.DeleteAllAsync();
                    await RespondAsync(command, "Datenbank wurde geleert.");
                    break;

                case "sub":
                    var added = await _subs.AddIfMissingAsync(command.User.Id);
                    await RespondAsync(command, added ? "Du wurdest eingetragen." : "Du bist bereits eingetragen.");
                    break;

                case "unsub":
                    var removed = await _subs.RemoveAsync(command.User.Id);
                    await RespondAsync(command, removed ? "Du wurdest ausgetragen." : "Du warst nicht eingetragen.");
                    break;

                default:
                    await RespondAsync(command, "Unbekannter Command.");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Slash command failed: {Command}", command.CommandName);
            await RespondAsync(command, "Fehler beim Ausfuehren des Commands.");
        }
    }

    private bool IsAdmin(ulong userId)
    {
        return userId == _options.AdminClientId;
    }
    
    private static async Task RespondAsync(SocketSlashCommand command, string text)
    {
        if (!command.HasResponded)
        {
            await command.RespondAsync(text, ephemeral: true);
            return;
        }

        await command.FollowupAsync(text, ephemeral: true);
    }
}