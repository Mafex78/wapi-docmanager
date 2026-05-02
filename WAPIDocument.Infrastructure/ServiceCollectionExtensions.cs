using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Shared.Infrastructure;
using WAPIDocument.Domain.Repositories;
using WAPIDocument.Infrastructure.Repositories;

namespace WAPIDocument.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWapiDocumentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mongoOptions = configuration
                               .GetSection(MongoOptions.SectionName)
                               .Get<MongoOptions>()
                           ?? new MongoOptions();
        
        ArgumentNullException.ThrowIfNull(mongoOptions);
        
        // Register mongoDb configuration as a singleton object
        services.AddSingleton<IMongoDatabase>(options =>
        {
            var client = new MongoClient(mongoOptions.ConnectionString);
            return client.GetDatabase(mongoOptions.Database);
        });
        
        services.AddScoped<IDocumentRepository, DocumentMongoRepository>();
        
        DocumentMap.Register();
        
        return services;
    }
}