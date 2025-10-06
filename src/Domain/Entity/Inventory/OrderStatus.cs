using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Inventory;

public class OrderStatus : Entity<string>
{
    public string Name { get; private set; } = null!;

    protected OrderStatus()
    {
    }

    public static OrderStatus Create(string name, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name);

        return new OrderStatus
        {
            Name = name,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void SetId(string id)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        Id = id;
    }

    public void Update(OrderStatus itemCategory)
    {
        DomainGuards.AgainstNullOrWhiteSpace(itemCategory.Name);
        Name = itemCategory.Name;
    }

    public bool HasChanges(OrderStatus? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return Name != other.Name;
    }
}