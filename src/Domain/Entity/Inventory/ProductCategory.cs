using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Inventory;

public class ProductCategory : Entity<string>
{
    public string Name { get; private set; } = null!;

    protected ProductCategory()
    {
    }

    public static ProductCategory Create(string name, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name);

        return new ProductCategory
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

    public void Update(ProductCategory productCategory)
    {
        DomainGuards.AgainstNullOrWhiteSpace(productCategory.Name);
        Name = productCategory.Name;
    }

    public bool HasChanges(ProductCategory? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return Name != other.Name;
    }
}