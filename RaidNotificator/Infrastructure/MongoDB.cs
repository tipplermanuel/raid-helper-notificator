using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;



namespace RaidNotificator.Infrastructure;

public class MongoDB
{
    private readonly ILogger<MongoDB> _logger;
    private readonly IConfiguration _configuration;
    private readonly IMongoClient _client;
    private readonly IMongoCollection<BsonDocument> _collection;
    
    public MongoDB(ILogger<MongoDB> logger, IConfiguration configuration)
    {
        if (Environment.GetEnvironmentVariable("ENV") == "PROD")
            _client = new MongoClient(configuration.GetConnectionString(configuration["MongoDBConnectionStringProd"] ?? throw new InvalidOperationException()));
        else
            _client = new MongoClient(configuration.GetConnectionString(configuration["MongoDBConnectionStringDev"] ?? throw new InvalidOperationException()));
        
        
        _collection = _client.GetDatabase("hausapotheke").GetCollection<BsonDocument>("events");
        
        _logger = logger;
        _configuration = configuration;
    }
}