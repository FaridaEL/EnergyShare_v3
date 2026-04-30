using Microsoft.EntityFrameworkCore;
using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Entities.Matchs;
using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Entities.ProfilsEnergie;
using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Domain.Entities.Messages;
using EnergyShare_v3.Domain.Entities.PointsAccesses;


namespace EnergyShare_v3.Application.Interfaces
{
    /*Contrat que la couche Infrastructure devra implementer.
    Definit les acces aux donnees dont l'Application a besoin.
    Pourquoi une interface pour le DbContext ?
/// - Permet de tester sans base de donnees reelle (mock)
/// - Respecte le principe d'inversion de dependance
/// - L'Application ne sait pas si on utilise SQL Server, PostgreSQL ou SQLite*/
    public interface IApplicationDbContext
    {   //Acces aux utilisateurs
        DbSet<DataPartage> DataPartages { get; }
        DbSet<DemandeGRD> DemandesGRD { get; } 
        DbSet<DocumentPartage> DocumentsPartage { get; }
        DbSet<Match> Matches { get; }
        DbSet<ParticipationPartage> MembresPartage { get; }
        DbSet<Message> Messages { get; }
        DbSet<OrganismePublic> OrganismesPublics { get; }
        DbSet<Partage> Partages { get; }
        DbSet<PointAccess> PointAccesses { get; }
        DbSet<ProfilEnergie> ProfilsEnergie { get; }
        DbSet<TarifAccord> TarifsAccord { get; }
        DbSet<User> Users { get; }

        DbSet<RefreshToken> RefreshTokens { get; }
        //à implémenter dans une v2 -> hors MVP
        //DbSet<FraisComptageMesurage> FraisComptageMesurage { get; }
        //DbSet<ProfilFacturation> ProfilsFacturation { get; }

        /// <summary>Sauvegarde les changements en base de donnees.</summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    }
}
