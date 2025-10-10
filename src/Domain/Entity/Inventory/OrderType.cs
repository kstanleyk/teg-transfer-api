using Transfer.Domain.Abstractions;

namespace Transfer.Domain.Entity.Inventory;

public class OrderType : Entity<string>
{
    public string Name { get; private set; } = null!;

    protected OrderType()
    {
    }

    public static OrderType Create(string name, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name);

        return new OrderType
        {
            Name = name,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(OrderType itemCategory)
    {
        DomainGuards.AgainstNullOrWhiteSpace(itemCategory.Name);
        Name = itemCategory.Name;
    }

    public bool HasChanges(OrderType? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return Name != other.Name;
    }
}