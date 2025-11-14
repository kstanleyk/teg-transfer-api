using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Auth;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Infrastructure.Persistence.Configurations.Core;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("client", SchemaNames.Core);

        // Configure custom properties (Identity properties are handled automatically)
        builder.Property(c => c.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.PhoneNumber)
            .HasMaxLength(35)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // ClientGroupId relationship
        builder.HasOne(c => c.ClientGroup)
            .WithMany(g => g.Clients)
            .HasForeignKey(c => c.ClientGroupId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Wallet relationship (one-to-one)
        builder.HasOne(c => c.Wallet)
            .WithOne()
            .HasForeignKey<Wallet>(w => w.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with ApplicationUser
        builder.HasOne(s => s.User)
            .WithOne(u => u.Client)
            .HasForeignKey<ApplicationUser>(u => u.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}