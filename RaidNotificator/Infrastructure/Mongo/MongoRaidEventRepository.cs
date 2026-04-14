using MongoDB.Driver;
using RaidNotificator.Contracts;
using RaidNotificator.DTOs;
using RaidNotificator.Infrastructure.Mongo.Documents;

namespace RaidNotificator.Infrastructure.Mongo;

public sealed class MongoRaidEventRepository(IMongoDatabase database) : IRaidEventRepository
{
    private readonly IMongoCollection<RaidEventDocument> _collection = database.GetCollection<RaidEventDocument>("events");

    public async Task UpsertAsync(RaidEvent raidEvent, CancellationToken cancellationToken = default)
    {
        var document = ToDocument(raidEvent);
        var filter = Builders<RaidEventDocument>.Filter.Eq(x => x.Id, raidEvent.Id);

        await _collection.ReplaceOneAsync(
            filter,
            document,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
    }

    public async Task<RaidEvent?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<RaidEventDocument>.Filter.Eq(x => x.Id, id);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return document == null ? null : ToDto(document);
    }

    public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        await _collection.DeleteManyAsync(Builders<RaidEventDocument>.Filter.Empty, cancellationToken);
    }

    private static RaidEventDocument ToDocument(RaidEvent dto)
    {
        return new RaidEventDocument
        {
            Id = dto.Id,
            Title = dto.Title,
            LastUpdated = dto.LastUpdated,
            SignUps = dto.SignUps.Select(s => new SignUpDocument
            {
                Name = s.Name,
                Id = s.Id,
                UserId = s.UserId,
                ClassName = s.ClassName,
                RoleName = s.RoleName,
                Status = s.Status,
                EntryTime = s.EntryTime,
                Position = s.Position,
                SpecName = s.SpecName,
                Spec2Name = s.Spec2Name
            }).ToList()
        };
    }

    private static RaidEvent ToDto(RaidEventDocument doc)
    {
        return new RaidEvent
        {
            Id = doc.Id,
            Title = doc.Title,
            LastUpdated = doc.LastUpdated,
            SignUps = doc.SignUps.Select(s => new SignUp
            {
                Name = s.Name,
                Id = s.Id,
                UserId = s.UserId,
                ClassName = s.ClassName,
                RoleName = s.RoleName,
                Status = s.Status,
                EntryTime = s.EntryTime,
                Position = s.Position,
                SpecName = s.SpecName,
                Spec2Name = s.Spec2Name
            }).ToList()
        };
    }
}
