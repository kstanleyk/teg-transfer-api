using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Core;

public class EstateTaskType : Entity<string>
{
    public Guid? PublicId { get; private set; }
    public string TaskTypeId { get; private set; } = null!;
    public string EstateId { get; private set; } = null!;
    public string AccountId { get; private set; } = null!;
    public DateTime EffectiveDate { get; private set; }
    public DateTime CreatedOn { get; private set; }

    protected EstateTaskType()
    {
    }

    public static EstateTaskType Create(
        string id,
        string taskTypeId,
        string estateId,
        string accountId,
        DateTime effectiveDate,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        DomainGuards.AgainstNullOrWhiteSpace(taskTypeId);
        DomainGuards.AgainstNullOrWhiteSpace(estateId);
        DomainGuards.AgainstNullOrWhiteSpace(accountId);

        return new EstateTaskType
        {
            Id = id, // Code → Id
            TaskTypeId = taskTypeId,
            EstateId = estateId,
            AccountId = accountId,
            EffectiveDate = effectiveDate,
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
