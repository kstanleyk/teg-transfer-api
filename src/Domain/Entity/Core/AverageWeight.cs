using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Core;

public class AverageWeight : Entity<string>
{
    public Guid? PublicId { get; private set; }
    public string Estate { get; private set; } = null!;
    public string Block { get; private set; } = null!;
    public double Weight { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public string Status { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

    protected AverageWeight()
    {
    }

    public static AverageWeight Create(string id, string estate, string block, double weight,
        DateTime effectiveDate, string status, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        DomainGuards.AgainstNullOrWhiteSpace(estate);
        DomainGuards.AgainstNullOrWhiteSpace(block);
        DomainGuards.AgainstNullOrWhiteSpace(status);

        if (weight < 0) throw new ArgumentOutOfRangeException(nameof(weight), "Weight cannot be negative.");

        return new AverageWeight
        {
            Id = id, // Code → Id
            Estate = estate,
            Block = block,
            Weight = weight,
            EffectiveDate = effectiveDate,
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

