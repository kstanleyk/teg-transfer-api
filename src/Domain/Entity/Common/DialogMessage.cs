using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Common;

public class DialogMessage : Entity<string>
{
    public string Lid { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

    protected DialogMessage()
    {
    }

    public static DialogMessage Create(string lid, string description, DateTime createdOn)
    {
        DomainGuards.AgainstNullOrWhiteSpace(lid);
        DomainGuards.AgainstNullOrWhiteSpace(description);

        return new DialogMessage
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