using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        
        services.AddDbContext<DocumentsDbContext>(opts => opts
            .UseMongoDB(mongoOptions.ConnectionString!, mongoOptions.Database!));
        
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
}