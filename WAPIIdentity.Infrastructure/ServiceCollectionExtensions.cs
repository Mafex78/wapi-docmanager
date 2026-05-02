using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
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
        
        // Register mongoDb configuration as a singleton object
        services.AddSingleton<IMongoDatabase>(options =>
        {
            var client = new MongoClient(mongoOptions.ConnectionString);
            return client.GetDatabase(mongoOptions.Database);
        });
        
        services.AddScoped<IUserRepository, UserMongoRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        
        UserMap.Register();
        
        return services;
    }
}