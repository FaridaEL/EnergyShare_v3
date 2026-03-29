using EnergyShare_v3.Domain.Entities.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Infrastructure.Database.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("messages");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.ObjetMessage)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(m => m.Contenu)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(m => m.IsLu)
                .IsRequired();

            builder.HasOne(m => m.Expediteur)
                .WithMany(u => u.MessagesEnvoyes)
                .HasForeignKey(m => m.ExpediteurId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Destinataire)
                .WithMany(u => u.MessagesRecus)
                .HasForeignKey(m => m.DestinataireId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Match)
                .WithMany()
                .HasForeignKey(m => m.MatchId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
