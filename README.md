# hausapotheke-raid-helper-notificator

Discord bot fuer Raid-Helper Event-Updates mit MongoDB Persistenz.

## Konfiguration

Per User Secrets oder Umgebungsvariablen:

- `DCToken`
- `GuildId`
- `ChannelName`
- `ConnectionStrings__MongoDBConnectionStringDev`
- `ConnectionStrings__MongoDBConnectionStringProd`
- optional: `RaidHelperBaseUrl` (default `https://raid-helper.xyz/api/v4/`)
- optional: `ENV` (`PROD` -> Prod-ConnectionString, sonst Dev)

## Slash Commands

- `/init` -> letzte 10 Nachrichten des konfigurierten Channels in DB syncen
- `/reset` -> Event-Datenbank leeren
- `/sub` -> Benutzer fuer DM-Benachrichtigungen eintragen
- `/unsub` -> Benutzer austragen

## Architektur

- `Hosting/DiscordBotHostedService` -> Discord Lifecycle
- `Discord/SlashCommand*` -> Registrierung + Verarbeitung von Commands
- `Application/*` -> Business-Logik
- `Infrastructure/*` -> API/Mongo
- `Contracts/*` -> Interfaces