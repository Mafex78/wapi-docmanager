using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WAPIIdentity.Application.Services;

namespace WAPIIdentity.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWapiIdentityApplication(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ILoginService, LoginService>();
        
        // Validators
        // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Scoped);
        services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies(), ServiceLifetime.Scoped);
        
        return services;
    }
}