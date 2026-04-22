using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WAPIDocument.Application.Services;

namespace WAPIDocument.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWapiDocumentApplication(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IDocumentService, DocumentService>();
        
        // Validators
        services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies(), ServiceLifetime.Scoped);
        
        return services;
    }
}