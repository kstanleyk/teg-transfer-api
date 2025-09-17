using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Core;

//Refactored from ActivityPlan
public class Task : Entity<string>
{
    public Guid? PublicId { get; private set; }
    public string Description { get; private set; } = null!;
    public string TaskType { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

    protected Task()
    {
    }

    public static Task Create(string description, string taskType, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(description);
        DomainGuards.AgainstNullOrWhiteSpace(taskType);

        return new Task
        {
            Description = description,
            TaskType = taskType,
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
