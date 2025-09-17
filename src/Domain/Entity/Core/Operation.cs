using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Core;

public class Operation : Entity<string>
{
    public string Line { get; private set; } = null!;
    public string Payroll { get; private set; } = null!; // FK
    public string Employee { get; private set; } = null!; // FK
    public string Estate { get; private set; } = null!; // FK
    public string Block { get; private set; } = null!; // FK
    public string Item { get; private set; } = null!; // FK TaskId
    public string MillingCycle { get; private set; } = null!; // FK
    public string Description { get; private set; } = null!;
    public double Quantity { get; private set; }
    public double Rate { get; private set; }
    public double Amount { get; private set; }
    public DateTime TransDate { get; private set; }
    public string Status { get; private set; } = null!;
    public string SyncReference { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

    protected Operation()
    {
    }

    public static Operation Create(
        string line,
        string payroll,
        string employee,
        string estate,
        string block,
        string item,
        string millingCycle,
        string description,
        double quantity,
        double rate,
        DateTime transDate,
        string status,
        string syncReference,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(line);
        DomainGuards.AgainstNullOrWhiteSpace(payroll);
        DomainGuards.AgainstNullOrWhiteSpace(employee);
        DomainGuards.AgainstNullOrWhiteSpace(estate);
        DomainGuards.AgainstNullOrWhiteSpace(block);
        DomainGuards.AgainstNullOrWhiteSpace(item);
        DomainGuards.AgainstNullOrWhiteSpace(millingCycle);
        DomainGuards.AgainstNullOrWhiteSpace(description);
        DomainGuards.AgainstNullOrWhiteSpace(status);
        DomainGuards.AgainstNullOrWhiteSpace(syncReference);

        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");
        if (rate < 0) throw new ArgumentOutOfRangeException(nameof(rate), "Rate cannot be negative.");

        return new Operation
        {
            Line = line,
            Payroll = payroll,
            Employee = employee,
            Estate = estate,
            Block = block,
            Item = item,
            MillingCycle = millingCycle,
            Description = description,
            Quantity = quantity,
            Rate = rate,
            Amount = quantity * rate,
            TransDate = transDate,
            Status = status,
            SyncReference = syncReference,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void SetId(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        Id = id;
    }
}
