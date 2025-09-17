using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Core;

public class Estate : Entity<string>
{
    public Guid? PublicId { get; private set; }
    public string Description { get; private set; } = null!;
    public string Location { get; private set; } = null!;
    public DateTime DateEstablished { get; private set; }
    public DateTime CreatedOn { get; private set; }

    protected Estate()
    {
    }

    public static Estate Create(
        string id,
        string description,
        string location,
        DateTime dateEstablished,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        DomainGuards.AgainstNullOrWhiteSpace(description);
        DomainGuards.AgainstNullOrWhiteSpace(location);

        return new Estate
        {
            Id = id, // Code → Id
            Description = description,
            Location = location,
            DateEstablished = dateEstablished,
            CreatedOn = createdOn ?? DateTime.UtcNow
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
