using Transfer.Domain.Abstractions;

namespace Transfer.Domain.Entity.Inventory;

public class Category : Entity<string>
{
    public string Name { get; private set; } = null!;

    protected Category()
    {
    }

    public static Category Create(string name, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name);

        return new Category
        {
            Name = name,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(Category category)
    {
        DomainGuards.AgainstNullOrWhiteSpace(category.Name);
        Name = category.Name;
    }

    public bool HasChanges(Category? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return Name != other.Name;
    }
}