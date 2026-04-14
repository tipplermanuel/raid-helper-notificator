using MongoDB.Bson.Serialization.Attributes;

namespace RaidNotificator.Infrastructure.Mongo.Documents;

public sealed class SubscriberDocument
{
    [BsonId]
    public ulong Id { get; set; }
}