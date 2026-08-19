using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProductReferenceProjectConfiguration : IEntityTypeConfiguration<ProductReferenceProject>
{
    public void Configure(EntityTypeBuilder<ProductReferenceProject> builder)
    {
        builder.ToTable("ProductReferenceProjects");

        builder.HasKey(prp => prp.Id);

        builder.HasIndex(prp => new { prp.ProductId, prp.ReferenceProjectId })
            .IsUnique();

        builder.HasOne(prp => prp.Product)
            .WithMany()
            .HasForeignKey(prp => prp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(prp => prp.ReferenceProject)
            .WithMany()
            .HasForeignKey(prp => prp.ReferenceProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
