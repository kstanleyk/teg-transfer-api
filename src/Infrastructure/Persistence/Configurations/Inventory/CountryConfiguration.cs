using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transfer.Domain.Entity.Inventory;

namespace Transfer.Infrastructure.Persistence.Configurations.Inventory;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        // Table name
        builder.ToTable("country", SchemaNames.Inventory);

        // Primary key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasMaxLength(2).IsRequired();

        // Name property
        builder.Property(c => c.Name).HasMaxLength(50).IsRequired();

        // PublicId
        builder.Property(c => c.PublicId).HasDefaultValueSql("gen_random_uuid()");

        // CreatedOn
        builder.Property(c => c.CreatedOn).IsRequired()
            .HasDefaultValueSql("NOW()"); // PostgreSQL: current UTC timestamp

        // Indexes
        builder.HasIndex(c => c.PublicId).IsUnique();
        builder.HasIndex(c => c.Name).IsUnique();
    }
}