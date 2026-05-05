using EnergyShare_v3.Domain.Entities.Partages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Infrastructure.Database.Configurations
{
    public class DataPartageConfiguration : IEntityTypeConfiguration<DataPartage>
    {
        public void Configure(EntityTypeBuilder<DataPartage> builder)
        {
            builder.ToTable("data_partage");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.VolumePartage_kWh)
                .HasPrecision(18, 4);

            builder.HasOne(d => d.PartageEnergie)
                .WithMany(p => p.RelevesSibelga)
                .HasForeignKey(d => d.PartageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
