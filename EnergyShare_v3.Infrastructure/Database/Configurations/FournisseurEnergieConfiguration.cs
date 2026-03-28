using EnergyShare_v3.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Infrastructure.Database.Configurations
{
    public class FournisseurEnergieConfiguration : IEntityTypeConfiguration<FournisseurEnergie>
    {
        public void Configure(EntityTypeBuilder<FournisseurEnergie> builder)
        {
            builder.ToTable("fournisseurs_energie");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Nom)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(f => f.Description)
                .HasMaxLength(1000);

            builder.Property(f => f.SiteWeb)
                .HasMaxLength(300);

            builder.Property(f => f.LogoUrl)
                .HasMaxLength(300);
        }
    }
}
