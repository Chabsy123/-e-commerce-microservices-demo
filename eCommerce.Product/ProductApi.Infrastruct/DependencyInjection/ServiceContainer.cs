using eCommerce.SharedLibrary.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using ProductApi.Application.Interface;
using ProductApi.Infrastruct.Data;
using ProductApi.Infrastruct.Repositories;

namespace ProductApi.Infrastruct.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructServices(this IServiceCollection services, IConfiguration config)
        {
            //Add Database connectivity
            // Add Authentication scheme

            SharedServiceContainer.AddSharedServices<ProductDbContext>(services, config, config["MySerilog:FileName"]!);

            //Create Dependency Injection
            services.AddScoped<IProduct, ProductRepository>();

            return services;
        }

        public static IApplicationBuilder UseInfrastructPolicy(this IApplicationBuilder app)
        {
            //Register middleware such as 
            // global exception: handles external errors
            // listen to only api gateway: blocks all outside calls;
            SharedServiceContainer.UseSharedPolicies(app);
            return app;
        }
    }
}
