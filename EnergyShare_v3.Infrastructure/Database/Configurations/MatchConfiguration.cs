using EnergyShare_v3.Domain.Entities.Matchs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyShare_v3.Infrastructure.Database.Configurations
{
    public class MatchConfiguration : IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> builder)
        {
            builder.ToTable("matches");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.DistanceCalculee)
                .IsRequired()
                .HasColumnType("decimal(18,4)");

            builder.HasOne(m => m.PointAccessVendeur)
                .WithMany(pa => pa.MatchsVendeurs)
                .HasForeignKey(m => m.PointAccessVendeurId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.PointAccessAcheteur)
                .WithMany(pa => pa.MatchsAcheteurs)
                .HasForeignKey(m => m.PointAccessAcheteurId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(m => new { m.PointAccessVendeurId, m.PointAccessAcheteurId })
                .IsUnique();

            builder.OwnsOne(m => m.Audit);
        }
    }
}
