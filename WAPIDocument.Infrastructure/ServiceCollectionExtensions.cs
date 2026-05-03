using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Shared.Domain;
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
        
        // Register all element for MongoDb - START *******
        services.AddSingleton<IMongoClient>(_ =>
            new MongoClient(mongoOptions.ConnectionString));
        
        // Register mongoDb configuration as a singleton object
        services.AddScoped<IMongoDatabase>(options =>
        {
            var client = options.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoOptions.Database);
        });
        
        services.AddScoped<IUnitOfWork, MongoUnitOfWork>();
        // Register all element for MongoDb - END *********
        
        services.AddScoped<IDocumentRepository, DocumentMongoRepository>();
        
        DocumentMap.Register();
        
        return services;
    }
}