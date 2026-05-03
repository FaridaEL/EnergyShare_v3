using EnergyShare_v3.Domain.Entities.Partages;
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

            builder.Property(p => p.Statut)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(40);

            builder.Property(p => p.EnergieType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(40);

            builder.Property(p => p.Perimetre)
                .HasConversion<string>()
                .HasMaxLength(40);

            builder.Property(p => p.InvitationCodeExpiresAt);

            builder.HasIndex(p => p.InvitationCode)
                .IsUnique()
                // L’index UNIQUE s’applique uniquement sur les lignes  où InvitationCode n’est PAS null
                // ça permet d’avoir plusieurs partages sans code, tout en garantissant
                // l’unicité des codes d’invitation lorsqu’ils existent.
                .HasFilter("[InvitationCode] IS NOT NULL");


            builder.HasOne(p => p.Vendeur)
                .WithMany(u => u.PartagesCrees)
                .HasForeignKey(p => p.VendeurId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.GestionnairePartage)
                .WithMany(u => u.PartagesGeres)
                .HasForeignKey(p => p.GestionnairePartageId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.OwnsOne(p => p.Audit);
        }
    }
}
