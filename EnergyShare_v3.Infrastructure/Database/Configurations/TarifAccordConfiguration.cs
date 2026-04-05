using EnergyShare_v3.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Infrastructure.Database.Configurations
{
    public class TarifAccordConfiguration : IEntityTypeConfiguration<TarifAccord>
    {
        public void Configure(EntityTypeBuilder<TarifAccord> builder)
        {
            builder.ToTable("tarifs_accord");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Montant)
                .IsRequired()
                .HasPrecision(18, 4);

            builder.HasOne(t => t.Partage)
                .WithMany(p => p.TarifsAccord)
                .HasForeignKey(t => t.PartageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.OwnsOne(t => t.Audit);
        }
    }
}
