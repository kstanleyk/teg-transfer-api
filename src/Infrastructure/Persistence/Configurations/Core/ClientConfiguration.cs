//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using TegWallet.Domain.Entity.Core;

//namespace TegWallet.Infrastructure.Persistence.Configurations.Core;

//public class ClientConfiguration : IEntityTypeConfiguration<Client>
//{
//    public void Configure(EntityTypeBuilder<Client> builder)
//    {
//        // Table name
//        builder.ToTable("client", SchemaNames.Core);

//        // Primary Key
//        builder.HasKey(c => c.Id);

//        // Properties
//        builder.Property(c => c.Email).IsRequired().HasMaxLength(255);
//        builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(20);
//        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
//        builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
//        builder.Property(c => c.CreatedAt).IsRequired();
//        builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

//        // Indexes
//        builder.HasIndex(c => c.Email).IsUnique().HasDatabaseName("ix_client_email");
//        builder.HasIndex(c => c.PhoneNumber).HasDatabaseName("ix_client_phone_number");
//        builder.HasIndex(c => new { c.FirstName, c.LastName }).HasDatabaseName("ix_client_name");
//        builder.HasIndex(c => c.Status).HasDatabaseName("ix_client_status");

//        // Relationships
//        builder.HasOne(c => c.Wallet)
//            .WithOne()
//            .HasForeignKey<Wallet>(w => w.ClientId)
//            .OnDelete(DeleteBehavior.Cascade);
//    }
//}