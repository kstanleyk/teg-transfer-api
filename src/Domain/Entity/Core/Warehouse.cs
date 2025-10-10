using Transfer.Domain.Abstractions;

namespace Transfer.Domain.Entity.Core;

public class Warehouse : Entity<string>
{
    public string Name { get; private set; } = null!;

    protected Warehouse()
    {
    }

    public static Warehouse Create(string name, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name);

        return new Warehouse
        {
            Name = name,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(Warehouse warehouse)
    {
        DomainGuards.AgainstNullOrWhiteSpace(warehouse.Name);

        Name = warehouse.Name;
    }

    public bool HasChanges(Warehouse? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return Name != other.Name;
    }
}