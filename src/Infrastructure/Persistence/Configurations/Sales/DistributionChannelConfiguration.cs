using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transfer.Domain.Entity.Sales;

namespace Transfer.Infrastructure.Persistence.Configurations.Sales;

public class DistributionChannelConfiguration : IEntityTypeConfiguration<DistributionChannel>
{
    public void Configure(EntityTypeBuilder<DistributionChannel> builder)
    {
        // Table name
        builder.ToTable("distribution_channel", SchemaNames.Sales);

        // Primary key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasMaxLength(5).IsRequired();

        // Name property
        builder.Property(c => c.Name).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(250).IsRequired();

        // PublicId
        builder.Property(c => c.PublicId).HasDefaultValueSql("gen_random_uuid()");

        // CreatedOn
        builder.Property(c => c.CreatedOn).IsRequired()
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(c => c.PublicId).IsUnique();
        builder.HasIndex(c => c.Name).IsUnique();
    }
}