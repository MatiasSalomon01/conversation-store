namespace WebApi.Services;

public class MongoDBService(string connectionString, string databaseName)
{
    private readonly IMongoDatabase _database = new MongoClient(connectionString).GetDatabase(databaseName);

    public IMongoCollection<Conversation> Conversations => _database.GetCollection<Conversation>("conversations");
}
