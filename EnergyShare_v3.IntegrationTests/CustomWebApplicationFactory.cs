using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnergyShare_v3.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");   // IMPORTANT : empêche Program.cs d’exécuter MigrateAsync()

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = "TEST_SECRET_KEY_12345678901234567890",
                    ["Jwt:Issuer"] = "EnergyShare.Tests",
                    ["Jwt:Audience"] = "EnergyShare.Tests",
                    ["Jwt:AccessTokenExpirationMinutes"] = "15",
                    ["Jwt:RefreshTokenExpirationDays"] = "7"
                });
            });

            builder.ConfigureServices(services =>
            { 
                
                // Supprimer tous les enregistrements liés au DbContext SQL Server
                services.RemoveAll<DbContextOptions>();
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();
                services.RemoveAll<IApplicationDbContext>();

                // Supprime aussi les configurations internes EF Core
                var efDescriptors = services
                    .Where(d =>
                        d.ServiceType.FullName != null &&
                        (d.ServiceType.FullName.Contains("DbContextOptions") ||
                         d.ServiceType.FullName.Contains("IDbContextOptionsConfiguration")))
                    .ToList();

                foreach (var descriptor in efDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Base SQLite en mémoire pour les tests
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                services.AddScoped<IApplicationDbContext>(sp =>
                    sp.GetRequiredService<ApplicationDbContext>());

                // Initialisation DB + seed
                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                db.Database.EnsureCreated();
                ApplicationDbContextSeeder.SeedAsync(db,userManager, roleManager)
                     .GetAwaiter()
                     .GetResult();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection?.Dispose();
        }
    }
}