using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Infrastructure
{
    // Methode d'extension pour enregistrer les services de l'Infrastructure.
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {   // Enregistrer Entity Framework Core avec SQL Server
            services.AddDbContext<ApplicationDBContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    //L'assembly des migrations est dansInfrastructure
                    sqlOptions.MigrationsAssembly(
                        typeof(IApplicationDbContext).Assembly.FullName);
                }
                ));
            // // Enregistrer IApplicationDbContext -> ApplicationDbContext
            // Quand quelqu'un demande IApplicationDbContext,
            // le conteneur DI fournit ApplicationDbContext

            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDBContext>());
            return services;


        }
    }
}
