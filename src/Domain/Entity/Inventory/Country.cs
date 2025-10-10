using Transfer.Domain.Abstractions;

namespace Transfer.Domain.Entity.Inventory;

public class Country : Entity<string>
{
    public string Name { get; private set; } = null!;

    protected Country()
    {
    }

    public static Country Create(string name, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name);

        return new Country
        {
            Name = name,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(Country country)
    {
        DomainGuards.AgainstNullOrWhiteSpace(country.Name);
        Name = country.Name;
    }

    public bool HasChanges(Country? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return Name != other.Name;
    }
}