using MongoDB.Bson.Serialization.Attributes;

namespace RaidNotificator.Infrastructure.Mongo.Documents;

public sealed class RaidEventDocument
{
    [BsonId]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public long LastUpdated { get; set; }

    public List<SignUpDocument> SignUps { get; set; } = new();
}

public sealed class SignUpDocument
{
    public string Name { get; set; } = string.Empty;
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long EntryTime { get; set; }
    public int Position { get; set; }
    public string? SpecName { get; set; }
    public string? Spec2Name { get; set; }
}