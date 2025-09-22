using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Inventory;

public class ItemCategory : Entity<string>
{
    public Guid? PublicId { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

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

    public void SetPublicId(Guid publicId)
    {
        ArgumentNullException.ThrowIfNull(publicId);
        PublicId = publicId;
    }
}