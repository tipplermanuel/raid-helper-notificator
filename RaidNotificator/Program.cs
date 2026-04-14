using System.Reflection;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using RaidNotificator.Application;
using RaidNotificator.Contracts;
using RaidNotificator.Discord;
using RaidNotificator.Hosting;
using RaidNotificator.Infrastructure.Mongo;
using RaidNotificator.Infrastructure.RaidHelper;
using RaidNotificator.Options;

var builder = Host.CreateApplicationBuilder(args);

// User Secrets + Env Vars
builder.Configuration
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables();

var botOptions = new BotOptions
{
    DiscordToken = builder.Configuration["DCToken"] ?? throw new InvalidOperationException("Missing DCToken."),
    GuildId = ulong.TryParse(builder.Configuration["GuildId"], out var guildId)
        ? guildId
        : throw new InvalidOperationException("GuildId missing or invalid."),
    ChannelName = builder.Configuration["ChannelName"] ?? throw new InvalidOperationException("Missing ChannelName."),
    AdminClientId = ulong.TryParse(builder.Configuration["AdminClientId"], out var adminId)
        ? adminId
        : throw new InvalidOperationException("AdminClientId missing or invalid."),
    RaidHelperBaseUrl = builder.Configuration["RaidHelperBaseUrl"] ?? "https://raid-helper.xyz/api/v4/"
};

if (!botOptions.RaidHelperBaseUrl.EndsWith("/"))
{
    botOptions.RaidHelperBaseUrl += "/";
}

// Einfacher ConnectionString
var mongoConnectionString = builder.Configuration["ConnectionString"] 
    ?? throw new InvalidOperationException("MongoDB ConnectionString missing.");

builder.Services.AddSingleton(botOptions);

// Discord
builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.All
}));

// Mongo
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase("hausapotheke"));

// Infra
builder.Services.AddHttpClient<IRaidHelperApiClient, RaidHelperApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<BotOptions>();
    client.BaseAddress = new Uri(options.RaidHelperBaseUrl);
});

// Contracts / Services
builder.Services.AddSingleton<IRaidEventRepository, MongoRaidEventRepository>();
builder.Services.AddSingleton<ISubscriberRepository, MongoSubscriberRepository>();
builder.Services.AddSingleton<IRegistrationDiffService, RegistrationDiffService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();

builder.Services.AddSingleton<RaidEventSyncService>();
builder.Services.AddSingleton<SlashCommandRegistrar>();
builder.Services.AddSingleton<SlashCommandHandler>();

builder.Services.AddHostedService<DiscordBotHostedService>();

await builder.Build().RunAsync();