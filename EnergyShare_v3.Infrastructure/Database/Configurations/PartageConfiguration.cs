using EnergyShare_v3.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Infrastructure.Database.Configurations
{
    public class PartageConfiguration : IEntityTypeConfiguration<Partage>
    {
        public void Configure(EntityTypeBuilder<Partage> builder)
        {
            builder.ToTable("partages");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Nom)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Description)
                .HasMaxLength(2000);

            builder.Property(p => p.RecevoirDataParticipant)
                .IsRequired();

            builder.Property(p => p.Statut)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(40);

            builder.Property(p => p.EnergieType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(40);

            builder.Property(p => p.DataTransmissionType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(40);

            builder.HasOne(p => p.Perimetre)
                .WithMany()
                .HasForeignKey(p => p.PerimetreId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Vendeur)
                .WithMany(u => u.PartagesCrees)
                .HasForeignKey(p => p.VendeurId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.GestionnairePartage)
                .WithMany(u => u.PartagesGeres)
                .HasForeignKey(p => p.GestionnairePartageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
