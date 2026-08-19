using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProductCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.ProductCode)
            .IsUnique();

        builder.Property(p => p.Brand)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.BrandValues)
            .IsRequired()
            .HasMaxLength(100);

        builder.Ignore(p => p.Brands);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.CommercialName)
            .HasMaxLength(250);

        builder.Property(p => p.ProductGroup)
            .HasMaxLength(150);

        builder.Property(p => p.Size)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.Surface)
            .HasMaxLength(100);

        builder.Property(p => p.Relief)
            .HasMaxLength(100);

        builder.Property(p => p.SpecialSurface)
            .HasMaxLength(100);

        builder.Property(p => p.Thickness)
            .HasColumnType("decimal(10,3)");

        builder.Property(p => p.BodyType)
            .HasMaxLength(100);

        builder.Property(p => p.Color)
            .HasMaxLength(100);

        builder.Property(p => p.ColorMaterial)
            .HasMaxLength(100);

        builder.Property(p => p.ApplicationArea)
            .HasMaxLength(100);

        builder.Property(p => p.UsageArea)
            .HasMaxLength(100);

        builder.Property(p => p.Finish)
            .HasMaxLength(100);

        builder.Property(p => p.PEI)
            .HasColumnType("decimal(10,3)");

        builder.Property(p => p.VValue)
            .HasMaxLength(20);

        builder.Property(p => p.RValue)
            .HasMaxLength(20);

        builder.Property(p => p.DeepAbrasion)
            .HasMaxLength(50);

        builder.Property(p => p.BoxM2)
            .HasColumnType("decimal(10,3)");

        builder.Property(p => p.PalletM2)
            .HasColumnType("decimal(10,3)");

        builder.Property(p => p.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Collection)
            .WithMany()
            .HasForeignKey(p => p.CollectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.SurfaceDefinition)
            .WithMany()
            .HasForeignKey(p => p.SurfaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.DisplayOrder)
            .IsUnique()
            .HasDatabaseName("UX_Products_DisplayOrder");

        builder.HasIndex(p => p.CreatedAt)
            .HasDatabaseName("IX_Products_CreatedAt");

        builder.HasIndex(p => new { p.Status, p.DisplayOrder })
            .HasDatabaseName("IX_Products_Status_DisplayOrder");

        builder.HasIndex(p => new { p.Status, p.CollectionId, p.DisplayOrder })
            .HasDatabaseName("IX_Products_Status_Collection_DisplayOrder");

        builder.HasIndex(p => new { p.Status, p.CategoryId, p.DisplayOrder })
            .HasDatabaseName("IX_Products_Status_Category_DisplayOrder");

        builder.HasIndex(p => new { p.Status, p.Brand, p.DisplayOrder })
            .HasDatabaseName("IX_Products_Status_Brand_DisplayOrder");
    }
}
