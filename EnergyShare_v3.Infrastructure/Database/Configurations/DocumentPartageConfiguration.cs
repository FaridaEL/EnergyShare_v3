using EnergyShare_v3.Domain.Entities.Partages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Infrastructure.Database.Configurations
{
    public class DocumentPartageConfiguration : IEntityTypeConfiguration<DocumentPartage>
    {
        public void Configure(EntityTypeBuilder<DocumentPartage> builder)
        {
            builder.ToTable("documents_partage");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.NomFichier)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.CheminStockage)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(d => d.TypeDocument)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(d => d.IsSigned)
                .IsRequired();

            builder.HasOne(d => d.Partage)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.PartageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.UploadedBy)
                .WithMany()
                .HasForeignKey(d => d.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.OwnsOne(d => d.Audit);
        }
    }
}
