using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Entities.ProfilsEnergie;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace EnergyShare_v3.Infrastructure.Database.Configurations
{

    public class PointAccessConfiguration : IEntityTypeConfiguration<PointAccess>
    {
        public void Configure(EntityTypeBuilder<PointAccess> builder)
        {
            builder.ToTable("point_accesses");

            builder.HasKey(pa => pa.Id);

            builder.Property(pa => pa.AdresseLine1)
                .HasMaxLength(255);

            builder.Property(pa => pa.CodePostal)
                .HasMaxLength(4);

            builder.Property(pa => pa.SmartMeter_Encrypted)
                .HasMaxLength(200);

            builder.Property(pa => pa.EAN_Encrypted)
                .HasMaxLength(200);

            builder.Property(pa => pa.IsInjectionPoint)
                .IsRequired();
           
            builder.Property(pa => pa.Fournisseur)
                 .IsRequired();

            builder.Property(pa => pa.AccordConsentement)
                .IsRequired();

            builder.HasOne(pa => pa.User)
                .WithMany(u => u.PointsAccess)
                .HasForeignKey(pa => pa.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pa => pa.ProfilEnergie)
                .WithOne(pe => pe.PointAccess)
                .HasForeignKey<ProfilEnergie>(pe => pe.PointAccessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.OwnsOne(pa => pa.Audit);
        }
    }
}
