using EnergyShare_v3.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyShare_v3.Infrastructure.Database.Configurations
{
    public class OrganismePublicConfiguration : IEntityTypeConfiguration<OrganismePublic>
    {
        public void Configure(EntityTypeBuilder<OrganismePublic> builder)
        {
            builder.ToTable("organismes_publics");

            builder.HasKey(op => op.Id);

            builder.Property(op => op.Nom)
                .IsRequired()
                .HasMaxLength(150);

            builder.OwnsOne(op => op.Audit);
        }
    }
}
