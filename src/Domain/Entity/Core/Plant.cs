using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Core;

public class Plant : Entity<string>
{
    public Guid? PublicId { get; private set; }
    public string Block { get; private set; } = null!;
    public double Number { get; private set; }
    public DateTime TransDate { get; private set; }
    public string Status { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

    protected Plant()
    {
    }

    public static Plant Create(
        string id,
        string block,
        double number,
        DateTime transDate,
        string status,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        DomainGuards.AgainstNullOrWhiteSpace(block);
        DomainGuards.AgainstNullOrWhiteSpace(status);

        if (number < 0) throw new ArgumentOutOfRangeException(nameof(number), "Number cannot be negative.");

        return new Plant
        {
            Id = id, // Code → Id
            Block = block,
            Number = number,
            TransDate = transDate,
            Status = status,
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
