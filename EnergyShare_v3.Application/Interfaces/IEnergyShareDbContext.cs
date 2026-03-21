using Microsoft.EntityFrameworkCore;
using EnergyShare_v3.Domain.Entities;


namespace EnergyShare_v3.Application.Interfaces
{
    /*Contrat que la couche Infrastructure devra implementer.
    Definit les acces aux donnees dont l'Application a besoin.
    Pourquoi une interface pour le DbContext ?
/// - Permet de tester sans base de donnees reelle (mock)
/// - Respecte le principe d'inversion de dependance
/// - L'Application ne sait pas si on utilise SQL Server, PostgreSQL ou SQLite*/
    public interface IEnergyShareDbContext
    {   //Acces aux utilisateurs
        DbSet<DataPartage> DataPartage { get; }
        DbSet<DdeInfoPerimetre> DdeInfoPerimetre { get; }
        DbSet<DdeValidationPartage> DdeValidationPartage { get; }
        DbSet<DocumentModele> DocumentModele { get; }
        DbSet<DocumentPartage> DocumentPartage { get; }
        DbSet<FournisseurEnergie> FournisseurEnergie { get; }
        DbSet<FraisComptageMesurage> FraisComptageMesurage { get; }
        DbSet<HistoriquePartageStatut> HistoriquePartageStatut { get; }
        DbSet<Match> Match { get; }
        DbSet<MembrePartage> MembrePartage { get; }
        DbSet<Message> Message { get; }
        DbSet<MethodeRepartitionInjection> MethodeRepartitionInjection { get; }
        DbSet<OrganismePublic> OrganismePublic { get; }
        DbSet<ParametreSysteme> ParametreSysteme { get; }
        DbSet<Partage> Partage { get; }
        DbSet<PerimetrePartageReglementaire> PerimetrePartageReglementaire { get; }
        DbSet<PointAccess> PointAccess { get; }
        DbSet<ProfilEnergie> ProfilEnergie { get; }
        DbSet<ProfilFacturation> ProfilFacturation { get; }
        DbSet<TarifAccord> TarifAccord { get; }
        DbSet<User> Users { get; }

        /// <summary>Sauvegarde les changements en base de donnees.</summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    }
}
