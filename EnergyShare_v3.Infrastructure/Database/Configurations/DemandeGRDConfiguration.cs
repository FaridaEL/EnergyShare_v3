using EnergyShare_v3.Domain.Entities.Partages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Infrastructure.Database.Configurations
{
    public class DemandeGRDConfiguration : IEntityTypeConfiguration<DemandeGRD>
    {
        public void Configure(EntityTypeBuilder<DemandeGRD> builder)
        {
            builder.ToTable("demandes_grd");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.DemandeType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(d => d.ResponseStatus)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(d => d.PerimetreConfirme)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(d => d.DetailsDemande)
                .HasMaxLength(2000);

            builder.Property(d => d.CommentaireReponseGRD)
                .HasMaxLength(2000);

            builder.HasOne(d => d.Demandeur)
                .WithMany()
                .HasForeignKey(d => d.DemandeurId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.AgentTraitant)
                .WithMany()
                .HasForeignKey(d => d.AgentTraitantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.OrganismePublic)
                .WithMany(op => op.DemandesGrd)
                .HasForeignKey(d => d.OrganismePublicId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Partage)
                .WithMany(p => p.DemandesGrd)
                .HasForeignKey(d => d.PartageId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
