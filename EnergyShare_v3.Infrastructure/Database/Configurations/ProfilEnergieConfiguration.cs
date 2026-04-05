using EnergyShare_v3.Domain.Entities.ProfilsEnergie;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace EnergyShare_v3.Infrastructure.Database.Configurations
{

    public class ProfilEnergieConfiguration : IEntityTypeConfiguration<ProfilEnergie>
    {
        public void Configure(EntityTypeBuilder<ProfilEnergie> builder)
        {
            builder.ToTable("profils_energie");

            builder.HasKey(pe => pe.Id);

            builder.Property(pe => pe.DemandeEnergie_kWh)
                .HasColumnType("decimal(18,4)");

            builder.Property(pe => pe.OffreEnergie_kWh)
                .HasColumnType("decimal(18,4)");

            builder.Property(pe => pe.PrixAchatCible_Eur)
                .HasColumnType("decimal(18,4)");

            builder.Property(pe => pe.PrixVenteCible_Eur)
                .HasColumnType("decimal(18,4)");

            builder.HasOne(pe => pe.PointAccess)
                .WithOne(pa => pa.ProfilEnergie)
                .HasForeignKey<ProfilEnergie>(pe => pe.PointAccessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.OwnsOne(pe => pe.Audit);
        }
    }
}
