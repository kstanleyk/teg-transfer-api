using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TegWallet.Domain.Entity.Auth;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Infrastructure.Persistence.Configurations.Auth;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("user", SchemaNames.Identity);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        // Relationship with Client
        builder.HasOne(u => u.Client)
            .WithOne(s => s.User)
            .HasForeignKey<Client>(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}