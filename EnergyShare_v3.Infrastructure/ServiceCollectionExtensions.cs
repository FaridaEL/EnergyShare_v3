using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Infrastructure.Behaviors;
using EnergyShare_v3.Infrastructure.Database;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EnergyShare_v3.Infrastructure
{
    /*  Méthode d’extension centrale pour enregistrer les services liés à l’Infrastructure. But :
        - garder Program.cs plus lisible ;
        - centraliser la configuration technique : Mediator, FluentValidation, EF Core, Identity ;
        - respecter l’inversion de dépendance : l’Application dépend de IApplicationDbContext et 
    l’Infrastructure fournit ApplicationDbContext.
   */
    public static class ServiceCollectionExtensions
    {    /* Assemblies où l’on cherche automatiquement :
             - les handlers Mediator ;
             - les validators FluentValidation.
         Application contient normalement les Commands, Queries, Handlers et Validators.  */

        /*private static readonly Assembly[] s_assemblies = [
            Assembly.Load("EnergyShare_v3.Application"),
            Assembly.Load("EnergyShare_v3.Domain"),
            ];*/
        private static readonly Assembly[] s_validationAssemblies =
        [
            Assembly.Load("EnergyShare_v3.Application"),
            Assembly.Load("EnergyShare_v3.Domain"),
        ];

        /*private static readonly Assembly[] s_validationAssemblies =
        [
            typeof(Application.Interfaces.IApplicationDbContext).Assembly,
            typeof(Domain.Entities.Users.User).Assembly
          ];   */

        public static IServiceCollection AddEnergyShare(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("EnergyShare")
                ?? throw new InvalidOperationException(
                    "La chaîne de connexion 'EnergyShare' est manquante.");

            return services
                .ConfigureMediator()
                .ConfigureFluentValidation()
                .ConfigureEntityFramework( connectionString)
                .ConfigureIdentity();

        }

        /* Configure Mediator.

        Rôle :
        - Mediator reçoit une requête/commande ;
        - il trouve automatiquement le handler correspondant ;
        - il exécute les PipelineBehaviors avant/après le handler.

        Important :
        RegisterServicesFromAssemblies permet d’éviter d’enregistrer manuellement :
        services.AddScoped<GetUsersHandler>();
        services.AddScoped<CreatePartageHandler>();
        etc.

        Donc TODO après cette configuratio : Suppprimer ces lignes dans Program.cs  si le projet compile.  */


        private static IServiceCollection ConfigureMediator(
            this IServiceCollection services)
        {
            return services.AddMediator(options =>
            {
                options.ServiceLifetime = ServiceLifetime.Scoped;
               // options.Assemblies = s_mediatorAssemblies;  // Indique à Mediator où chercher les handlers.--> Sans cela on doit les enregistrer à la main dans Program.cs.

                /* PipelineBehaviors :
                - LoggingBehavior : journalise l’exécution des requêtes ;
                - ValidationBehavior : exécute FluentValidation avant le handler ;
                - UnitOfWorkBehavior : sauvegarde les changements après le handler.

                Attention TODO à vérifier : Si TransactionBehavior ET UnitOfWorkBehavior appellent tous les deux SaveChangesAsync,
                Il faut garder seulement UnitOfWorkBehavior pour éviter les doublons.
               */
                options.PipelineBehaviors = [
                    typeof(LoggingBehavior<,>),
                    typeof(ValidationBehavior<,>),
                    typeof(TransactionBehavior<,>), // to do vérifier si double sauvegarde avec UnitOfWorkBehavior
                    typeof(UnitOfWorkBehavior<,>)// à conserver si c’est lui qui fait SaveChangesAsync une seule fois.
                    ];
            });
        }

        /* Configure FluentValidation -->  Rôle :
            - détecter automatiquement les validators dans Application/Domain ;
            - les enregistrer dans le conteneur DI ;
            - permettre au ValidationBehavior de les exécuter automatiquement.
        Ex : CreatePartageCommandValidator sera appelé avant CreatePartageHandler.  */

        private static IServiceCollection ConfigureFluentValidation(
          this IServiceCollection services)
            {
                foreach (var result in AssemblyScanner
                    .FindValidatorsInAssemblies(s_validationAssemblies))
                {
                    services.AddTransient(
                        result.InterfaceType,
                        result.ValidatorType);
                }

            /*   CascadeMode.Stop : si une règle échoue, FluentValidation arrête les règles suivantes
         sur la même propriété. --> Avantage : messages d’erreur plus lisibles et moins redondants. */
            ValidatorOptions.Global.DefaultRuleLevelCascadeMode =
                    CascadeMode.Stop;

                return services;
            }

        /*Configure EF Core --> Rôle :
          - enregistrer ApplicationDbContext ;
          - préciser SQL Server comme provider ;
          - exposer ApplicationDbContext via IApplicationDbContext.
         Cela permet à la couche Application de dépendre de l’interface IApplicationDbContext sans connaître EF Core directement.   */
        private static IServiceCollection ConfigureEntityFramework(
            this IServiceCollection services,
            string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            /* Variante explicite et robuste : si un service demande IApplicationDbContext,
            on lui donne l’instance courante d’ApplicationDbContext.        */
            //services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());
            return services;
        }

        /*Configure ASP.NET Identity --> Rôle :
                - gérer les utilisateurs ;
                - gérer les rôles ;
                - gérer le hash des mots de passe ;
                - gérer lockout, tokens, sécurité, etc.     */
        private static IServiceCollection ConfigureIdentity(
           this IServiceCollection services)
        {
            services
                .AddIdentity<User, IdentityRole<Guid>>(options =>
                {   // Politique de mot de passe : vérifier si conforme aux règles de sécurité du CDC--> TODO : adapter si plus stricte 
                    options.Password.RequiredLength = 8;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;

                    options.Lockout.MaxFailedAccessAttempts = 5; // Protection contre les attaques par force brute.Après 5 échecs, le compte est temporairement verrouillé.
                   options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                    options.Lockout.AllowedForNewUsers = true;

                    options.User.RequireUniqueEmail = true;//empêche deux compte d'avoir le meme mail
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}
