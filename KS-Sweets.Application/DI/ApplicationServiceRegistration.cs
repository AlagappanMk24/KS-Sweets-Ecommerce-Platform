using KS_Sweets.Application.Mappings;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace KS_Sweets.Application.DI
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        {
            // Configuration of AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(Assembly.GetExecutingAssembly());
            });

            return services;
        }
    }
}
