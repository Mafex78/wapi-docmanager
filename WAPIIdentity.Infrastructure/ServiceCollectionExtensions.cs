using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        
        services.AddDbContext<IdentityDbContext>(opts => opts
            .UseMongoDB(mongoOptions.ConnectionString!, mongoOptions.Database!));
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        
        return services;
    }
}