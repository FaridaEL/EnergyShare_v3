using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EnergyShare_v3.Infrastructure.Database
{   
    // Implementation concrete de IApplicationDbContext avec Entity Framework Core.

    public class EnergyShareDBContext : DbContext, IEnergyShareDbContext
    {
        public EnergyShareDBContext(DbContextOptions<EnergyShareDBContext> options)
          : base(options) { 
        
        }

        public DbSet<DataPartage> DataPartage { get; set; } = null;
        public DbSet<DdeInfoPerimetre> DdeInfoPerimetre { get; set; } = null;
        public DbSet<DdeValidationPartage> DdeValidationPartage { get; set; } = null;
        public DbSet<DocumentModele> DocumentModele { get; set; } = null;
        public DbSet<DocumentPartage> DocumentPartage { get; set; } = null;
        public DbSet<FournisseurEnergie> FournisseurEnergie { get; set; } = null;
        public DbSet<FraisComptageMesurage> FraisComptageMesurage { get; set; } = null;
        public DbSet<HistoriquePartageStatut> HistoriquePartageStatut { get; set; } = null;
        public DbSet<Match> Match { get; set; } = null;
        public DbSet<MembrePartage> MembrePartage { get; set; } = null;
        public DbSet<Message> Message { get; set; } = null;
        public DbSet<MethodeRepartitionInjection> MethodeRepartitionInjection { get; set; } = null;
        public DbSet<OrganismePublic> OrganismePublic { get; set; } = null;
        public DbSet<ParametreSysteme> ParametreSysteme { get; set; } = null;
        public DbSet<Partage> Partage { get; set; } = null;
        public DbSet<PerimetrePartageReglementaire> PerimetrePartageReglementaire { get; set; } = null;
        public DbSet<PointAccess> PointAccess { get; set; } = null;
        public DbSet<ProfilEnergie> ProfilEnergie { get; set; } = null;
        public DbSet<ProfilFacturation> ProfilFacturation { get; set; } = null;
        public DbSet<TarifAccord> TarifAccord { get; set; } = null;
        public DbSet<User> Users { get; set; } = null;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Applique automatiquement toutes les configurations
            // trouvees dans cet assembly (les classes IEntityTypeConfiguration<T>)
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }



    }
}
