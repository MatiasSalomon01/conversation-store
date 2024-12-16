namespace WebApi.Configuration;

public static class MongoDBConfiguration
{
    public static IServiceCollection AddMongoDB(this IServiceCollection services, IConfiguration configuration)
    {
        var camelCaseConventionPack = new ConventionPack() { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("CamelCase", camelCaseConventionPack, type => true);

        return services.AddSingleton(serviceProvider =>
        {
            //var database = serviceProvider.GetRequiredService<IOptions<Database>>().Value;
            var database = configuration.GetSection(nameof(Database)).Get<Database>()!;
            return new MongoDBService(database.ConnectionString, database.Name);
        });
    }
}
