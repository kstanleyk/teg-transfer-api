using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Core;

public class ExpenseType : Entity<string>
{
    public Guid? PublicId { get; private set; }
    public string Description { get; private set; } = null!;
    public string Account { get; private set; } = null!;
    public string InventoryStatus { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

    // Navigation property
    private readonly List<ExpenseTypeInventory> _expenseTypeInventories = [];
    public IReadOnlyCollection<ExpenseTypeInventory> ExpenseTypeInventories => _expenseTypeInventories.AsReadOnly();

    protected ExpenseType()
    {
    }

    public static ExpenseType Create(
        string id,
        string description,
        string account,
        string inventoryStatus,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        DomainGuards.AgainstNullOrWhiteSpace(description);
        DomainGuards.AgainstNullOrWhiteSpace(account);
        DomainGuards.AgainstNullOrWhiteSpace(inventoryStatus);

        return new ExpenseType
        {
            Id = id,
            Description = description,
            Account = account,
            InventoryStatus = inventoryStatus,
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

    public void AddExpenseTypeInventory(ExpenseTypeInventory inventory)
    {
        if (inventory is null) throw new ArgumentNullException(nameof(inventory));
        _expenseTypeInventories.Add(inventory);
    }
}
