using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Entities.ProfilsEnergie;
using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Infrastructure.Database
{
    public static class ApplicationDbContextSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Users.AnyAsync())
                return;

            var sibelga = new OrganismePublic
            {
                Id = Guid.NewGuid(),
                Nom = "Sibelga"
            };

            context.OrganismesPublics.Add(sibelga);

            var vendeur1 = User.Create(
                "sarah.dupont@example.com",
                "HASH_TEMPORAIRE",
                UserRole.Utilisateur).Value;

            vendeur1.UpdateUserIdentity("Sarah", "Dupont", "0470000001");

            var vendeur2 = User.Create(
                "julien.martin@example.com",
                "HASH_TEMPORAIRE",
                UserRole.Utilisateur).Value;

            vendeur2.UpdateUserIdentity("Julien", "Martin", "0470000002");

            var acheteur1 = User.Create(
                "lea.bernard@example.com",
                "HASH_TEMPORAIRE",
                UserRole.Utilisateur).Value;

            acheteur1.UpdateUserIdentity("Léa", "Bernard", "0470000003");

            var acheteur2 = User.Create(
                "hugo.lambert@example.com",
                "HASH_TEMPORAIRE",
                UserRole.Utilisateur).Value;

            acheteur2.UpdateUserIdentity("Hugo", "Lambert", "0470000004");

            var boulangerie = User.Create(
                "contact@boulangerie-dupain.be",
                "HASH_TEMPORAIRE",
                UserRole.Utilisateur).Value;

            boulangerie.UpdateUserIdentity(null, null, "0220000010");
            boulangerie.UpdateLegalInformation("Boulangerie Du Pain", "BE0123456789");

            var agentSibelga = User.Create(
                "agent.sibelga@example.com",
                "HASH_TEMPORAIRE",
                UserRole.OrganismePublic).Value;

            agentSibelga.UpdateUserIdentity("Nadia", "Vermeulen", "0220000099");
            agentSibelga.OrganismePublicId = sibelga.Id;

            context.Users.AddRange(vendeur1, vendeur2, acheteur1, acheteur2, boulangerie, agentSibelga);

            var paVendeur1 = new PointAccess
            {
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
            };

            var paVendeur2 = new PointAccess
            {
                Id = Guid.NewGuid(),
                AdresseLine1 = "Avenue Louise 210",
                CodePostal = "1050",
                Latitude = 50.8229,
                Longitude = 4.3661,
                IsInjectionPoint = true,
                Fournisseur = "Luminus",
                SmartMeter_Encrypted = "1SJ-VENDEUR-0002",
                EAN_Encrypted = "541448900000000002",
                UserId = vendeur2.Id
            };

            var paAcheteur1 = new PointAccess
            {
                Id = Guid.NewGuid(),
                AdresseLine1 = "Chaussée de Wavre 88",
                CodePostal = "1040",
                Latitude = 50.8360,
                Longitude = 4.3833,
                IsInjectionPoint = false,
                Fournisseur = "TotalEnergies",
                SmartMeter_Encrypted = "1SJ-ACHETEUR-0001",
                EAN_Encrypted = "541448900000000003",
                UserId = acheteur1.Id
            };

            var paAcheteur2 = new PointAccess
            {
                Id = Guid.NewGuid(),
                AdresseLine1 = "Rue Haute 155",
                CodePostal = "1000",
                Latitude = 50.8385,
                Longitude = 4.3444,
                IsInjectionPoint = false,
                Fournisseur = "Mega",
                SmartMeter_Encrypted = "1SJ-ACHETEUR-0002",
                EAN_Encrypted = "541448900000000004",
                UserId = acheteur2.Id
            };

            var paBoulangerie = new PointAccess
            {
                Id = Guid.NewGuid(),
                AdresseLine1 = "Rue du Pain 12",
                CodePostal = "1000",
                Latitude = 50.8478,
                Longitude = 4.3495,
                IsInjectionPoint = false,
                Fournisseur = "Engie",
                SmartMeter_Encrypted = "1SJ-BOULANGERIE-01",
                EAN_Encrypted = "541448900000000005",
                UserId = boulangerie.Id
            };

            context.PointAccesses.AddRange(
                paVendeur1,
                paVendeur2,
                paAcheteur1,
                paAcheteur2,
                paBoulangerie);

            var profilVendeur1 = ProfilEnergie.Create(
                demande: null,
                offre: 3200,
                prixAchatCible: null,
                prixVenteCible: 0.12m,
                pointAccessId: paVendeur1.Id).Value;

            var profilVendeur2 = ProfilEnergie.Create(
                demande: null,
                offre: 4200,
                prixAchatCible: null,
                prixVenteCible: 0.11m,
                pointAccessId: paVendeur2.Id).Value;

            var profilAcheteur1 = ProfilEnergie.Create(
                demande: 1800,
                offre: null,
                prixAchatCible: 0.16m,
                prixVenteCible: null,
                pointAccessId: paAcheteur1.Id).Value;

            var profilAcheteur2 = ProfilEnergie.Create(
                demande: 2400,
                offre: null,
                prixAchatCible: 0.15m,
                prixVenteCible: null,
                pointAccessId: paAcheteur2.Id).Value;

            var profilBoulangerie = ProfilEnergie.Create(
                demande: 5200,
                offre: null,
                prixAchatCible: 0.18m,
                prixVenteCible: null,
                pointAccessId: paBoulangerie.Id).Value;

            context.ProfilsEnergie.AddRange(
                profilVendeur1,
                profilVendeur2,
                profilAcheteur1,
                profilAcheteur2,
                profilBoulangerie);

            await context.SaveChangesAsync();
        }
    }
}
