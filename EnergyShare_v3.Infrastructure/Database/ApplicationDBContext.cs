using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Entities.Matchs;
using EnergyShare_v3.Domain.Entities.Messages;
using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Entities.ProfilsEnergie;
using EnergyShare_v3.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using EnergyShare_v3.Bricks.Model;

namespace EnergyShare_v3.Infrastructure.Database
{   
    // Implementation concrete de IApplicationDbContext avec Entity Framework Core.

    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options) { 
        
        }
        public DbSet<DataPartage> DataPartages { get; set; } = null!;
        public DbSet<DdeInfoPerimetre> DdesInfoPerimetre { get; set; } = null!;
        public DbSet<DdeValidationPartage> DdesValidationPartage { get; set; } = null!;
        public DbSet<DocumentModele> DocumentsModele { get; set; } = null!;
        public DbSet<DocumentPartage> DocumentsPartage { get; set; } = null!;
        public DbSet<FournisseurEnergie> FournisseursEnergie { get; set; } = null!;
        public DbSet<FraisComptageMesurage> FraisComptageMesurage { get; set; } = null!;
        public DbSet<HistoriquePartageStatut> HistoriquesPartageStatut { get; set; } = null!;
        public DbSet<Match> Matches { get; set; } = null!;
        public DbSet<MembrePartage> MembresPartage { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<MethodeRepartitionInjection> MethodesRepartitionInjection { get; set; } = null!;
        public DbSet<OrganismePublic> OrganismesPublics { get; set; } = null!;
        public DbSet<ParametreSysteme> ParametresSysteme { get; set; } = null!;
        public DbSet<Partage> Partages { get; set; } = null!;
        public DbSet<PerimetrePartageReglementaire> PerimetresPartageReglementaire { get; set; } = null!;
        public DbSet<PointAccess> PointAccesses { get; set; } = null!;
        public DbSet<ProfilEnergie> ProfilsEnergie { get; set; } = null!;
        public DbSet<ProfilFacturation> ProfilsFacturation { get; set; } = null!;
        public DbSet<TarifAccord> TarifsAccord { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null! ;



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Applique automatiquement toutes les configurations
            // trouvees dans cet assembly (les classes IEntityTypeConfiguration<T>)
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Mapping de AuditInfo comme objet possédé (Owned Type).
            // Les champs d'audit sont stockés dans la même table que l'entité propriétaire..
            //Todo: dès que j'aurai des données de tests, je devrais retirer les audit.Touch() dans mes entités et handler
            modelBuilder.Entity<User>().OwnsOne(x => x.Audit);
            modelBuilder.Entity<ProfilEnergie>().OwnsOne(x => x.Audit);
            modelBuilder.Entity<Match>().OwnsOne(x => x.Audit);
            modelBuilder.Entity<Partage>().OwnsOne(x => x.Audit);
            modelBuilder.Entity<DocumentModele>().OwnsOne(x => x.Audit);
        }


        // Intercepte chaque sauvegarde EF Core pour appliquer automatiquement l'audit
        // avant d'envoyer les données en base (évite de le faire manuellement partout).
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            return await base.SaveChangesAsync(cancellationToken);
        }

        // Parcourt toutes les entités modifiées ou créées
        // et met à jour automatiquement les informations d'audit (dates, user).

        private void ApplyAuditInfo()
        {
            // Récupère toutes les entités suivies par EF qui implémentent IAuditable
            // et qui sont en cours de création ou modification.
            var auditableEntries = ChangeTracker
                .Entries<IAuditable>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in auditableEntries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.Audit.SetCreated(null);
                    entry.Entity.Audit.Touch(null);
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.Audit.Touch(null);
                }
            }
        }



    }
}
