using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Shared.Domain;
using Shared.Infrastructure;
using WAPIIdentity.Application.Services;
using WAPIIdentity.Domain.Repositories;
using WAPIIdentity.Infrastructure.Repositories;

namespace WAPIIdentity.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWapiIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        
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
        
        services.AddScoped<IUserRepository, UserMongoRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        
        UserMap.Register();
        
        return services;
    }
}