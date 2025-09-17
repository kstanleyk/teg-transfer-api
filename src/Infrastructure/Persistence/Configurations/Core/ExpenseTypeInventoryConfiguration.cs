using Agrovet.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrovet.Infrastructure.Persistence.Configurations.Core;

public class ExpenseTypeInventoryConfiguration : IEntityTypeConfiguration<ExpenseTypeInventory>
{
    public void Configure(EntityTypeBuilder<ExpenseTypeInventory> entity)
    {
        entity.HasKey(e => new { e.Id, e.ExpenseType, e.InventoryItem });
        entity.HasIndex(c => c.PublicId).IsUnique();

        entity.Property(e => e.Id).IsRequired().HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.PublicId).HasColumnType("uuid").IsRequired().IsUnicode(false);
        entity.Property(e => e.ExpenseType).IsRequired().HasMaxLength(5).IsUnicode(false);
        entity.Property(e => e.InventoryItem).IsRequired().HasMaxLength(5).IsUnicode(false);

        entity.ToTable("expense_type_inventory", SchemaNames.Core);
    }
}