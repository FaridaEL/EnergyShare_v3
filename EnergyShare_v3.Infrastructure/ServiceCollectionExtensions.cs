using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Infrastructure.Behaviors;
using EnergyShare_v3.Infrastructure.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EnergyShare_v3.Infrastructure
{
    // Methode d'extension pour enregistrer les services de l'Infrastructure.
    public static class ServiceCollectionExtensions
    {
        static readonly List<Assembly> s_assemblies = [
            Assembly.Load("EnergyShare_v3.Application"),
            Assembly.Load("EnergyShare_v3.Domain"),

            ];


        public static IServiceCollection AddEnergyShare(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services
                .ConfigureMediator()
                .ConfigureFluentValidation()
                .ConfigureEntityFramework(
                   configuration.GetConnectionString("EnergyShare")!);

        }
        static IServiceCollection ConfigureMediator(
            this IServiceCollection services)
        {
            return services.AddMediator(options =>
            {
                options.ServiceLifetime = ServiceLifetime.Scoped;
                options.PipelineBehaviors = [
                    typeof(LoggingBehavior<,>),
                    typeof(ValidationBehavior<,>),
                    typeof(TransactionBehavior<,>),
                    typeof(UnitOfWorkBehavior<,>)
                    ];
            });
        }

        static IServiceCollection ConfigureFluentValidation(
      this IServiceCollection services)
        {
            foreach (var result in AssemblyScanner
                .FindValidatorsInAssemblies(s_assemblies))
            {
                services.AddTransient(
                    result.InterfaceType,
                    result.ValidatorType);
            }

            ValidatorOptions.Global.DefaultRuleLevelCascadeMode =
                CascadeMode.Stop;

            return services;
        }

        static IServiceCollection ConfigureEntityFramework(
            this IServiceCollection services,
            string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

            return services;
        }
    }
}
