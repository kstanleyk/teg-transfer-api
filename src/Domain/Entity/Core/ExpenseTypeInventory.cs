using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Core;

public class ExpenseTypeInventory : Entity<string>
{
    public Guid? PublicId { get; private set; }
    public string ExpenseType { get; private set; } = null!;
    public string InventoryItem { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

    protected ExpenseTypeInventory()
    {
    }

    public static ExpenseTypeInventory Create(
        string id,
        string expenseType,
        string inventoryItem,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        DomainGuards.AgainstNullOrWhiteSpace(expenseType);
        DomainGuards.AgainstNullOrWhiteSpace(inventoryItem);

        return new ExpenseTypeInventory
        {
            Id = id, // Code → Id
            ExpenseType = expenseType,
            InventoryItem = inventoryItem,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void SetId(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        Id = id;
    }

    public void SetPublicId(Guid publicId)
    {
        ArgumentNullException.ThrowIfNull(publicId);
        PublicId = publicId;
    }
}