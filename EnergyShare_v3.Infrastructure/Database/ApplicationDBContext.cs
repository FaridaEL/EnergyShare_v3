using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Entities.Matchs;
using EnergyShare_v3.Domain.Entities.Messages;
using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Entities.ProfilsEnergie;
using EnergyShare_v3.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EnergyShare_v3.Infrastructure.Database
{   
    // Implementation concrete de IApplicationDbContext avec Entity Framework Core.

    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options) { 
        
        }

        public DbSet<DataPartage> DataPartages { get; set; } = null;
        public DbSet<DdeInfoPerimetre> DdesInfoPerimetre { get; set; } = null;
        public DbSet<DdeValidationPartage> DdesValidationPartage { get; set; } = null;
        public DbSet<DocumentModele> DocumentsModele { get; set; } = null;
        public DbSet<DocumentPartage> DocumentsPartage { get; set; } = null;
        public DbSet<FournisseurEnergie> FournisseursEnergie { get; set; } = null;
        public DbSet<FraisComptageMesurage> FraisComptageMesurage { get; set; } = null;
        public DbSet<HistoriquePartageStatut> HistoriquesPartageStatut { get; set; } = null;
        public DbSet<Match> Matches { get; set; } = null;
        public DbSet<MembrePartage> MembresPartage { get; set; } = null;
        public DbSet<Message> Messages { get; set; } = null;
        public DbSet<MethodeRepartitionInjection> MethodesRepartitionInjection { get; set; } = null;
        public DbSet<OrganismePublic> OrganismesPublics { get; set; } = null;
        public DbSet<ParametreSysteme> ParametresSysteme { get; set; } = null;
        public DbSet<Partage> Partages { get; set; } = null;
        public DbSet<PerimetrePartageReglementaire> PerimetresPartageReglementaire { get; set; } = null;
        public DbSet<PointAccess> PointAccesses { get; set; } = null;
        public DbSet<ProfilEnergie> ProfilsEnergie { get; set; } = null;
        public DbSet<ProfilFacturation> ProfilsFacturation { get; set; } = null;
        public DbSet<TarifAccord> TarifsAccord { get; set; } = null;
        public DbSet<User> Users { get; set; } = null ;



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Applique automatiquement toutes les configurations
            // trouvees dans cet assembly (les classes IEntityTypeConfiguration<T>)
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }



    }
}
