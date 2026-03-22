using EnergyShare_v3.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Infrastructure.Database
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()  // Stocke l'enum en tant que texte
                .HasMaxLength(20);

            // (un email est unique au sein d'une famille)
            builder.HasIndex(u => new { u.Email })
                .IsUnique();
            /* ex. prof à reprendre pour l'EAN unique au sein d'un partage et ne participe qu'à un seul partage.
             * // Index unique sur Email + FamilyId
             // (un email est unique au sein d'une famille)
             builder.HasIndex(m => new { m.Email, m.FamilyId })
                 .IsUnique();

         }    */
        }
    }
}
