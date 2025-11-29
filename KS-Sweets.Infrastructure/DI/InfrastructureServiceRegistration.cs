using KS_Sweets.Application.Contracts.Services;
using KS_Sweets.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KS_Sweets.Infrastructure.DI
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, EmailService>();
            return services;
        }
    }
}