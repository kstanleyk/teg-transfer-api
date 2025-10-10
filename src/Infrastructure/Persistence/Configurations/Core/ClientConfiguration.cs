using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transfer.Domain.Entity.Core;

namespace Transfer.Infrastructure.Persistence.Configurations.Core;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        // Table name
        builder.ToTable("client", SchemaNames.Core);

        // Primary Key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Email).IsRequired().HasMaxLength(255);
        builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        // Data Objects (if any embedded in Client)
        // In this case, Client doesn't have direct value objects, but has navigation properties

        // Indexes
        builder.HasIndex(c => c.Email).IsUnique().HasDatabaseName("IX_Clients_Email");
        builder.HasIndex(c => c.PhoneNumber).HasDatabaseName("IX_Clients_PhoneNumber");
        builder.HasIndex(c => new { c.FirstName, c.LastName }).HasDatabaseName("IX_Clients_Name");
        builder.HasIndex(c => c.Status).HasDatabaseName("IX_Clients_Status");

        // Relationships
        builder.HasOne(c => c.Wallet)
            .WithOne()
            .HasForeignKey<Wallet>(w => w.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filter for soft delete (if implemented)
        // builder.HasQueryFilter(c => c.Status != ClientStatus.Deleted);
    }
}