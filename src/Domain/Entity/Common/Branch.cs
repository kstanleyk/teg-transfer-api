using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Common;

public class Branch : Entity<string>
{
    public string Description { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string Telephone { get; private set; } = null!;
    public Guid? PublicId { get; private set; }
    public DateTime CreatedOn { get; private set; }

    protected Branch()
    {
    }

    public static Branch Create(string description, string address, string telephone, DateTime createdOn)
    {
        DomainGuards.AgainstNullOrWhiteSpace(description);
        DomainGuards.AgainstNullOrWhiteSpace(address);
        DomainGuards.AgainstNullOrWhiteSpace(telephone);

        return new Branch
        {
            Description = description,
            Address = address,
            Telephone = telephone,
            CreatedOn = createdOn
        };
    }

    public void SetId(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        Id = id;
    }

    public void SetPublicId(Guid publicId)
    {
        ArgumentNullException.ThrowIfNull(publicId);
        PublicId = publicId;
    }
}
