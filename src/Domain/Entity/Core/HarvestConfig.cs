namespace Agrovet.Domain.Entity.Core;

public class HarvestConfig
{
    public Guid? PublicId { get; private set; }
    public string HarvestId { get; private set; } = null!;
    public string CarryingId { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

    protected HarvestConfig()
    {
    }

    public static HarvestConfig Create(
        string harvestId,
        string carryingId,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(harvestId);
        DomainGuards.AgainstNullOrWhiteSpace(carryingId);

        return new HarvestConfig
        {
            HarvestId = harvestId, // HarvestId → Id
            CarryingId = carryingId,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void SetPublicId(Guid publicId)
    {
        ArgumentNullException.ThrowIfNull(publicId);
        PublicId = publicId;
    }
}
