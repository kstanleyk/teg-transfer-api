using Transfer.Domain.Abstractions;

namespace Transfer.Domain.Entity.Inventory;

public class ProductMovement : Entity<string>
{
    public string LineNum { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Item { get; private set; } = null!;
    public DateTime TransDate { get; private set; }
    public string TransTime { get; private set; } = null!;
    public string Sense { get; private set; } = null!; // e.g., "IN" / "OUT"
    public double Qtty { get; private set; }
    public string SourceId { get; private set; } = null!;
    public string SourceLineNum { get; private set; } = null!;

    protected ProductMovement() { }

    public static ProductMovement Create(string lineNum, string description, string item, DateTime transDate,
        string transTime, string sense, double qtty, string sourceId, string sourceLineNum, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(lineNum);
        DomainGuards.AgainstNullOrWhiteSpace(description);
        DomainGuards.AgainstNullOrWhiteSpace(item);
        DomainGuards.AgainstNullOrWhiteSpace(transTime);
        DomainGuards.AgainstNullOrWhiteSpace(sense);
        DomainGuards.AgainstNullOrWhiteSpace(sourceId);
        DomainGuards.AgainstNullOrWhiteSpace(sourceLineNum);

        if (qtty <= 0)
            throw new ArgumentOutOfRangeException(nameof(qtty), "Quantity must be greater than zero.");

        if (sense != "D" && sense != "C")
            throw new ArgumentException("Sense must be either 'D' or 'C'.", nameof(sense));

        return new ProductMovement
        {
            LineNum = lineNum,
            Description = description,
            Item = item,
            TransDate = transDate,
            TransTime = transTime,
            Sense = sense,
            Qtty = qtty,
            SourceId = sourceId,
            SourceLineNum = sourceLineNum,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void ReverseMovement()
    {
        Sense = Sense == MovementDirection.Inward ? MovementDirection.Outward : MovementDirection.Inward;
    }

    internal static class MovementDirection
    {
        public const string Inward = "D";
        public const string Outward = "C";
    }
}