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
    public interface IApplicationDbContext
    {   //Acces aux utilisateurs
        DbSet<DataPartage> DataPartages { get; }
        DbSet<DdeInfoPerimetre> DdesInfoPerimetre { get; }
        DbSet<DdeValidationPartage> DdesValidationPartage { get; }
        DbSet<DocumentModele> DocumentsModele { get; }
        DbSet<DocumentPartage> DocumentsPartage { get; }
        DbSet<FournisseurEnergie> FournisseursEnergie { get; }
        DbSet<FraisComptageMesurage> FraisComptageMesurage { get; }
        DbSet<HistoriquePartageStatut> HistoriquesPartageStatut { get; }
        DbSet<Match> Matches { get; }
        DbSet<MembrePartage> MembresPartage { get; }
        DbSet<Message> Messages { get; }
        DbSet<MethodeRepartitionInjection> MethodesRepartitionInjection { get; }
        DbSet<OrganismePublic> OrganismesPublics { get; }
        DbSet<ParametreSysteme> ParametresSysteme { get; }
        DbSet<Partage> Partages { get; }
        DbSet<PerimetrePartageReglementaire> PerimetresPartageReglementaire { get; }
        DbSet<PointAccess> PointAccesses { get; }
        DbSet<ProfilEnergie> ProfilsEnergie { get; }
        DbSet<ProfilFacturation> ProfilsFacturation { get; }
        DbSet<TarifAccord> TarifsAccord { get; }
        DbSet<User> Users { get; }

        /// <summary>Sauvegarde les changements en base de donnees.</summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    }
}
