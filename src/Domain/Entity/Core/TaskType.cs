using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Core;

public class TaskType : Entity<string>
{
    public Guid? PublicId { get; private set; }
    public string Description { get; private set; } = null!;
    public string Account { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

    protected TaskType()
    {
    }

    public static TaskType Create(string id, string description, string account, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        DomainGuards.AgainstNullOrWhiteSpace(description);
        DomainGuards.AgainstNullOrWhiteSpace(account);

        return new TaskType
        {
            Id = id, // Code → Id
            Description = description,
            Account = account,
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
