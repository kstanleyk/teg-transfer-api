using Transfer.Domain.Abstractions;

namespace Transfer.Domain.Entity.Common;

public class MonthName : Entity<string>
{
    public string Lid { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

    protected MonthName()
    {
    }

    public static MonthName Create(string lid, string description, DateTime createdOn)
    {
        DomainGuards.AgainstNullOrWhiteSpace(lid);
        DomainGuards.AgainstNullOrWhiteSpace(description);

        return new MonthName
        {
            Lid = lid,
            Description = description,
            CreatedOn = createdOn
        };
    }

    public void SetId(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        Id = id;
    }
}