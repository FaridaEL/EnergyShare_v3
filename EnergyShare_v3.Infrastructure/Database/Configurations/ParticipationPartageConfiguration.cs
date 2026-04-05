using EnergyShare_v3.Domain.Entities.Partages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyShare_v3.Infrastructure.Database.Configurations
{
    public class ParticipationPartageConfiguration : IEntityTypeConfiguration<ParticipationPartage>
    {
        public void Configure(EntityTypeBuilder<ParticipationPartage> builder)
        {
            builder.ToTable("participations_partage");

            builder.HasKey(pp => pp.Id);

            builder.Property(pp => pp.IsInterlocuteurUnique)
                .IsRequired();

            builder.Property(pp => pp.UserRolePartage)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasOne(pp => pp.Partage)
                .WithMany(p => p.Membres)
                .HasForeignKey(pp => pp.PartageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pp => pp.PointAccess)
                .WithMany(pa => pa.Membres)
                .HasForeignKey(pp => pp.PointAccessId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pp => new { pp.PartageId, pp.PointAccessId })
                .IsUnique();

            builder.OwnsOne(pp => pp.Audit);
        }
    }
}

