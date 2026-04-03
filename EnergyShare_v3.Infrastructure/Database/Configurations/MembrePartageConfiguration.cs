using EnergyShare_v3.Domain.Entities.Partages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Infrastructure.Database.Configurations
{
    public class MembrePartageConfiguration : IEntityTypeConfiguration<MembrePartage>
    {
        public void Configure(EntityTypeBuilder<MembrePartage> builder)
        {
            builder.ToTable("membres_partage");

            builder.HasKey(mp => mp.Id);

            builder.Property(mp => mp.IsInterlocuteurUnique)
                .IsRequired();

            builder.Property(mp => mp.UserRolePartage)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasOne(mp => mp.User)
                .WithMany(u => u.MembresPartage)
                .HasForeignKey(mp => mp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(mp => mp.Partage)
                .WithMany(p => p.Membres)
                .HasForeignKey(mp => mp.PartageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(mp => mp.PointAccess)
                .WithMany(pa => pa.Membres)
                .HasForeignKey(mp => mp.PointAccessId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(mp => new { mp.PartageId, mp.PointAccessId })
                .IsUnique();

            builder.OwnsOne(mp => mp.Audit);
        }
    }
}

