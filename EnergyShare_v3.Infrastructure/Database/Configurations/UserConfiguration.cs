using EnergyShare_v3.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Infrastructure.Database.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.Property(u => u.FirstName)
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .HasMaxLength(100);

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(50);

            builder.Property(u => u.SocieteName)
                .HasMaxLength(200);

            builder.Property(u => u.NumeroEntreprise)
                .HasMaxLength(12);

            builder.Property(u => u.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(u => u.UserType)
                .IsRequired()
                .HasConversion<string>()   // Stocke l'enum en tant que texte
                .HasMaxLength(50);

            builder.Property(u => u.FormeLegaleType)
                .HasConversion<string>()
                .HasMaxLength(50);

            // builder.HasIndex(u => u.Email)      //email unique au sein de l'application
            //    .IsUnique();       //Doit se faire sur un index , pas sur une propriété simple, d'où l'utilisation d'une propriété de valeur (Owned Entity) pour l'email dans la v2 ci-dessous

            builder.OwnsOne(u => u.Email, owned =>
            {
                owned.Property(e => e.Value)
                    .HasColumnName("Email")
                    .HasMaxLength(200)
                    //.IsUnique()
                    .IsRequired();

                owned.HasIndex(e => e.Value).IsUnique();//L'unicité se définit sur un index, pas sur une propriété
            });

            builder.HasOne(u => u.OrganismePublic)
                .WithMany()
                .HasForeignKey(u => u.OrganismePublicId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.OwnsOne(u => u.Audit);
        }

      
          
    }
}

  
