using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Infrastructure.Persistence.Configurations.Core;

public class ClientGroupConfiguration : IEntityTypeConfiguration<ClientGroup>
{
    public void Configure(EntityTypeBuilder<ClientGroup> builder)
    {
        builder.ToTable("client_group", SchemaNames.Core);

        // Primary Key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever().IsRequired();

        // Properties
        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("IX_ClientGroups_Name");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_ClientGroups_IsActive");

        // Check constraints
        builder.HasCheckConstraint("CK_ClientGroups_Name_Length", "LEN([Name]) >= 2 AND LEN([Name]) <= 50");

        // Navigation properties will be configured in other configurations
    }
}