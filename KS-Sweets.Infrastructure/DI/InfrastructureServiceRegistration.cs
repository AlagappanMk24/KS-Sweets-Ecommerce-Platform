using KS_Sweets.Application.Contracts.Persistence;
using KS_Sweets.Application.Contracts.Services;
using KS_Sweets.Infrastructure.Data.Initializers;
using KS_Sweets.Infrastructure.Persistence;
using KS_Sweets.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KS_Sweets.Infrastructure.DI
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
        {
            // Register foundational services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDbInitializer, DbInitializer>();

            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IEmailService, EmailService>();
            return services;
        }
    }
}