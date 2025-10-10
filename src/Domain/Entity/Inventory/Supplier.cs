using Transfer.Domain.Abstractions;

namespace Transfer.Domain.Entity.Inventory;

public class Supplier : Entity<string>
{
    public string Name { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string ContactPerson { get; private set; } = null!;

    protected Supplier()
    {
    }

    public static Supplier Create(
        string name,
        string address,
        string city,
        string phone,
        string contactPerson,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name);
        DomainGuards.AgainstNullOrWhiteSpace(address);
        DomainGuards.AgainstNullOrWhiteSpace(city);
        DomainGuards.AgainstNullOrWhiteSpace(phone);
        DomainGuards.AgainstNullOrWhiteSpace(contactPerson);

        return new Supplier
        {
            Name = name,
            Address = address,
            City = city,
            Phone = phone,
            ContactPerson = contactPerson,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void SetId(string id)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        Id = id;
    }

    public void Update(Supplier supplier)
    {
        DomainGuards.AgainstNullOrWhiteSpace(supplier.Name);
        DomainGuards.AgainstNullOrWhiteSpace(supplier.Address);
        DomainGuards.AgainstNullOrWhiteSpace(supplier.City);
        DomainGuards.AgainstNullOrWhiteSpace(supplier.Phone);
        DomainGuards.AgainstNullOrWhiteSpace(supplier.ContactPerson);

        PublicId = supplier.PublicId;
        Name = supplier.Name;
        Address = supplier.Address;
        City = supplier.City;
        Phone = supplier.Phone;
        ContactPerson = supplier.ContactPerson;
    }

    public bool HasChanges(Supplier? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return Name != other.Name ||
               Address != other.Address ||
               City != other.City ||
               Phone != other.Phone ||
               ContactPerson != other.ContactPerson;
    }
}
