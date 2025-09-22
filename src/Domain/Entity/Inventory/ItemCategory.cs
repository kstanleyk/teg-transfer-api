using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Inventory;

public class ItemCategory : Entity<string>
{
    public string Name { get; private set; } = null!;

    protected ItemCategory()
    {
    }

    public static ItemCategory Create(string name, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name);

        return new ItemCategory
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

    public void Update(ItemCategory itemCategory)
    {
        DomainGuards.AgainstNullOrWhiteSpace(itemCategory.Name);
        Name = itemCategory.Name;
    }

    public bool HasChanges(ItemCategory? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return Name != other.Name;
    }
}