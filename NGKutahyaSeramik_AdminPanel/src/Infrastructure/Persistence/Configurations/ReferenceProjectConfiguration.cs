using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ReferenceProjectConfiguration : IEntityTypeConfiguration<ReferenceProject>
{
    public void Configure(EntityTypeBuilder<ReferenceProject> builder)
    {
        builder.ToTable("ReferenceProjects");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.Location)
            .HasMaxLength(300);

        builder.Property(rp => rp.Region)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(rp => rp.Brand)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(rp => rp.ProjectType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(rp => rp.Architect)
            .HasMaxLength(200);

        builder.Property(rp => rp.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(rp => rp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(rp => rp.DisplayOrder)
            .IsUnique()
            .HasDatabaseName("UX_ReferenceProjects_DisplayOrder");
    }
}
