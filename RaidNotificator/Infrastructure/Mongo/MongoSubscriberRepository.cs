using MongoDB.Driver;
using RaidNotificator.Contracts;
using RaidNotificator.DTOs;
using RaidNotificator.Infrastructure.Mongo.Documents;

namespace RaidNotificator.Infrastructure.Mongo;

public sealed class MongoSubscriberRepository : ISubscriberRepository
{
    private readonly IMongoCollection<SubscriberDocument> _collection;

    public MongoSubscriberRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<SubscriberDocument>("subscribers");
    }

    public async Task<IReadOnlyCollection<Subscriber>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var docs = await _collection.Find(Builders<SubscriberDocument>.Filter.Empty)
            .ToListAsync(cancellationToken);

        return docs.Select(x => new Subscriber { Id = x.Id }).ToList();
    }

    public async Task<bool> AddIfMissingAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        var exists = await _collection.Find(x => x.Id == userId).AnyAsync(cancellationToken);
        if (exists)
        {
            return false;
        }

        await _collection.InsertOneAsync(new SubscriberDocument { Id = userId }, cancellationToken: cancellationToken);
        return true;
    }

    public async Task<bool> RemoveAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == userId, cancellationToken);
        return result.DeletedCount > 0;
    }
}