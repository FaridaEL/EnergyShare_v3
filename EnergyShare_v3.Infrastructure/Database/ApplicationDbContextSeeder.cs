using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Entities.PointsAccesses;
using EnergyShare_v3.Domain.Entities.ProfilsEnergie;
using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Infrastructure.Database
{
    public static class ApplicationDbContextSeeder
    {     // Mot de passe de test pour tous les users : Test1234
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            await SeedRolesAsync(roleManager);

            if (await context.PointAccesses.AnyAsync())
                return;

            var sibelga = await context.OrganismesPublics
                .FirstOrDefaultAsync(x => x.Nom == "Sibelga");

            if (sibelga is null)
            {
                sibelga = new OrganismePublic
                {
                    Id = Guid.NewGuid(),
                    Nom = "Sibelga"
                };

                context.OrganismesPublics.Add(sibelga);
                await context.SaveChangesAsync();
            }

            var vendeur1 = await CreateUserIfNotExistsAsync(
                userManager,
                "sarah.dupont@example.com",
                "Test1234",
                UserRole.Utilisateur,
                "Sarah",
                "Dupont",
                "0470000001",
                "Utilisateur");

            var vendeur2 = await CreateUserIfNotExistsAsync(
                userManager,
                "julien.martin@example.com",
                "Test1234",
                UserRole.Utilisateur,
                "Julien",
                "Martin",
                "0470000002",
                "Utilisateur");

            var acheteur1 = await CreateUserIfNotExistsAsync(
                userManager,
                "lea.bernard@example.com",
                "Test1234",
                UserRole.Utilisateur,
                "Léa",
                "Bernard",
                "0470000003",
                "Utilisateur");

            var acheteur2 = await CreateUserIfNotExistsAsync(
                userManager,
                "hugo.lambert@example.com",
                "Test1234",
                UserRole.Utilisateur,
                "Hugo",
                "Lambert",
                "0470000004",
                "Utilisateur");

            var boulangerie = await CreateUserIfNotExistsAsync(
                userManager,
                "contact@boulangerie-dupain.be",
                "Test1234",
                UserRole.Utilisateur,
                null,
                null,
                "0220000010",
                "Utilisateur");

            boulangerie.UpdateLegalInformation("Boulangerie Du Pain", "BE0123456789");
            await userManager.UpdateAsync(boulangerie);

            var agentSibelga = await CreateUserIfNotExistsAsync(
                userManager,
                "agent.sibelga@example.com",
                "Test1234",
                UserRole.OrganismePublic,
                "Nadia",
                "Vermeulen",
                "0220000099",
                "OrganismePublic");

            agentSibelga.OrganismePublicId = sibelga.Id;
            await userManager.UpdateAsync(agentSibelga);

            var paVendeur1 = PointAccess.Create(
                vendeur1.Id,
                "Rue des Fleurs 12",
                "1000",
                "Engie",
                "1SJ-VENDEUR-0001",
                "541448900000000001",
                true).Value;

            paVendeur1.SetCoordinates(50.8466, 4.3528); 
            /*{ // on passe désomrais par la factrory create car propriétés passées en private set
                Id = Guid.NewGuid(),
                AdresseLine1 = "Rue des Fleurs 12",
                CodePostal = "1000",
                Latitude = 50.8466,
                Longitude = 4.3528,
                IsInjectionPoint = true,
                Fournisseur = "Engie",
                SmartMeter_Encrypted = "1SJ-VENDEUR-0001",
                EAN_Encrypted = "541448900000000001",
                UserId = vendeur1.Id
            };*/

            var paVendeur2 = PointAccess.Create(
                vendeur2.Id,
                "Avenue Louise 210",
                "1050",
                "Luminus",
                "1SJ-VENDEUR-0002",
                "541448900000000002",
                true).Value;
            paVendeur2.SetCoordinates(50.8229, 4.3661);


            var paAcheteur1 = PointAccess.Create(
    acheteur1.Id,
    "Chaussée de Wavre 88",
    "1040",
    "TotalEnergies",
    "1SJ-ACHETEUR-0001",
    "541448900000000003",
    false).Value;
            paAcheteur1.SetCoordinates(50.8360, 4.3833);

            var paAcheteur2 = PointAccess.Create(
                acheteur2.Id,
                "Rue Haute 155",
                "1000",
                "Mega",
                "1SJ-ACHETEUR-0002",
                "541448900000000004",
                false).Value;
            paAcheteur2.SetCoordinates(50.8385, 4.3444);

            var paBoulangerie = PointAccess.Create(
                boulangerie.Id,
                "Rue du Pain 12",
                "1000",
                "Engie",
                "1SJ-BOULANGERIE-01",
                "541448900000000005",
                false).Value;
            paBoulangerie.SetCoordinates(50.8478, 4.3495);

            context.PointAccesses.AddRange(
                paVendeur1,
                paVendeur2,
                paAcheteur1,
                paAcheteur2,
                paBoulangerie);

            var profilVendeur1 = ProfilEnergie.Create(null, 3200, null, 0.12m, paVendeur1.Id).Value;
            var profilVendeur2 = ProfilEnergie.Create(null, 4200, null, 0.11m, paVendeur2.Id).Value;
            var profilAcheteur1 = ProfilEnergie.Create(1800, null, 0.16m, null, paAcheteur1.Id).Value;
            var profilAcheteur2 = ProfilEnergie.Create(2400, null, 0.15m, null, paAcheteur2.Id).Value;
            var profilBoulangerie = ProfilEnergie.Create(5200, null, 0.18m, null, paBoulangerie.Id).Value;

            context.ProfilsEnergie.AddRange(
                profilVendeur1,
                profilVendeur2,
                profilAcheteur1,
                profilAcheteur2,
                profilBoulangerie);

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            string[] roles = ["Administrateur", "OrganismePublic", "Utilisateur"];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>
                    {
                        Name = role
                    });
                }
            }
        }

        private static async Task<User> CreateUserIfNotExistsAsync(
            UserManager<User> userManager,
            string email,
            string password,
            UserRole businessRole,
            string? firstName,
            string? lastName,
            string? phoneNumber,
            string? identityRole = null)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser is not null)
                return existingUser;

            var result = User.Create(email, businessRole);
            if (!result.IsSuccess)
                throw new InvalidOperationException($"Impossible de créer l'utilisateur {email}");

            var user = result.Value;
            user.UpdateUserIdentity(firstName, lastName, phoneNumber);

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Erreur création utilisateur {email}: {errors}");
            }

            if (!string.IsNullOrWhiteSpace(identityRole))
            {
                var roleResult = await userManager.AddToRoleAsync(user, identityRole);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Erreur rôle utilisateur {email}: {errors}");
                }
            }

            return user;
        }
    }
}
