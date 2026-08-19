using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Aynı etiketin tekrar oluşturulmasını engeller — BlogService.ResolveTagIdsAsync
        // (get-or-create) bu index'e güvenir.
        builder.HasIndex(t => t.Name)
            .IsUnique();
    }
}
