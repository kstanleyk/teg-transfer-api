using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Core;

public class ExpenseStatus : Entity<string>
{
    public Guid? PublicId { get; private set; }
    public string Description { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

    protected ExpenseStatus()
    {
    }

    public static ExpenseStatus Create(string id, string description, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        DomainGuards.AgainstNullOrWhiteSpace(description);

        return new ExpenseStatus
        {
            Id = id, // Code → Id
            Description = description,
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
