namespace Agrovet.Domain.Entity.Core;

public class TaskTypeAccount
{
    public string TaskType { get; private set; } = null!;
    public string Estate { get; private set; } = null!;
    public string Account { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

    protected TaskTypeAccount()
    {
    }

    public static TaskTypeAccount Create(
        string taskType,
        string estate,
        string account,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(taskType);
        DomainGuards.AgainstNullOrWhiteSpace(estate);
        DomainGuards.AgainstNullOrWhiteSpace(account);

        return new TaskTypeAccount
        {
            TaskType = taskType,
            Estate = estate,
            Account = account,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }
}
