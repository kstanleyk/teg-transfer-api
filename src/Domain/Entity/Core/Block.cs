using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Core;

public class Block : Entity<string>
{
    public Guid? PublicId { get; private set; }
    public string Estate { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public double TreeNumber { get; private set; }
    public DateTime DateEstablished { get; private set; }
    public double BlockSize { get; private set; }
    public DateTime CreatedOn { get; private set; }

    protected Block()
    {
    }

    public static Block Create(
        string id,
        string estate,
        string description,
        double treeNumber,
        DateTime dateEstablished,
        double blockSize,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        DomainGuards.AgainstNullOrWhiteSpace(estate);
        DomainGuards.AgainstNullOrWhiteSpace(description);

        if (treeNumber < 0) throw new ArgumentOutOfRangeException(nameof(treeNumber), "Tree number cannot be negative.");
        if (blockSize < 0) throw new ArgumentOutOfRangeException(nameof(blockSize), "Block size cannot be negative.");

        return new Block
        {
            Id = id, // Code → Id
            Estate = estate,
            Description = description,
            TreeNumber = treeNumber,
            DateEstablished = dateEstablished,
            BlockSize = blockSize,
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
