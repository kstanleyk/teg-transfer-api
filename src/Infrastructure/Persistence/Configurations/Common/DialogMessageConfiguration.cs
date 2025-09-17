using Agrovet.Domain.Entity.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Common;

public class DialogMessageConfiguration : IEntityTypeConfiguration<DialogMessage>
{
    public void Configure(EntityTypeBuilder<DialogMessage> entity)
    {
        entity.HasKey(e => new {e.Id, e.Lid});

        entity.Property(e => e.Id).HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.Lid).HasMaxLength(2).IsUnicode(false);
        entity.Property(e => e.Description).IsRequired().HasMaxLength(125).IsUnicode(false);

        entity.ToTable("dialog_message", SchemaNames.Common);
    }
}