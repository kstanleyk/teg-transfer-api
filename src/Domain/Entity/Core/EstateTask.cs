using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Core;

/// <summary>
/// the task that are applicable to each estate, with the rate
/// </summary>
public class EstateTask : Entity<string>
{
    public Guid? PublicId { get; private set; }
    public string TaskId { get; private set; } = null!;
    public string EstateId { get; private set; } = null!;
    public double Rate { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public DateTime CreatedOn { get; private set; }

    protected EstateTask()
    {
    }

    public static EstateTask Create(
        string id,
        string taskId,
        string estateId,
        double rate,
        DateTime effectiveDate,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        DomainGuards.AgainstNullOrWhiteSpace(taskId);
        DomainGuards.AgainstNullOrWhiteSpace(estateId);

        if (rate < 0) throw new ArgumentOutOfRangeException(nameof(rate), "Rate cannot be negative.");

        return new EstateTask
        {
            Id = id, // Code → Id
            TaskId = taskId,
            EstateId = estateId,
            Rate = rate,
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
