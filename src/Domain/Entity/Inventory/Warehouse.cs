using Transfer.Domain.Abstractions;
using Transfer.Domain.ValueObjects;

namespace Transfer.Domain.Entity.Inventory;

public class Warehouse : Entity<string>
{
    public string Name { get; private set; } = null!;
    public Address Address { get; private set; } = null!;

    protected Warehouse()
    {
    }

    public static Warehouse Create(string name, Address address, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name);

        return new Warehouse
        {
            Name = name,
            Address = address,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(Warehouse warehouse)
    {
        DomainGuards.AgainstNullOrWhiteSpace(warehouse.Name);

        Name = warehouse.Name;
        Address = warehouse.Address;
    }

    public bool HasChanges(Warehouse? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return Name != other.Name || !Address.Equals(other.Address);
    }
}