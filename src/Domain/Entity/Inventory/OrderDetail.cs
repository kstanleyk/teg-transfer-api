using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Inventory;

public class OrderDetail : Entity<string>
{
    public string LineNum { get; private set; } = null!;
    public string Item { get; private set; } = null!;
    public DateTime? ExpiryDate { get; private set; }
    public string Description { get; private set; } = null!;
    public DateTime? ReceiveDate { get; private set; }
    public double Qtty { get; private set; }
    public double UnitCost { get; private set; }
    public double Amount { get; private set; }
    public string Status { get; private set; } = null!;
    public DateTime TransDate { get; private set; }

    // Navigation back to Order (aggregate root)
    public string OrderId { get; private set; } = null!;
    public Order Order { get; private set; } = null!;

    protected OrderDetail() { }

    public static OrderDetail Create(
        string item,
        double qtty,
        double unitCost,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(item);

        if (qtty <= 0)
            throw new ArgumentOutOfRangeException(nameof(qtty), "Quantity must be greater than zero.");

        if (unitCost < 0)
            throw new ArgumentOutOfRangeException(nameof(unitCost), "Unit cost cannot be negative.");

        return new OrderDetail
        {
            Item = item,
            Qtty = qtty,
            UnitCost = unitCost,
            Amount = qtty * unitCost,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void SetId(string id)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        Id = id;
    }

    public void UpdateQuantity(double newQtty)
    {
        if (newQtty <= 0)
            throw new ArgumentOutOfRangeException(nameof(newQtty), "Quantity must be greater than zero.");

        Qtty = newQtty;
        Amount = Qtty * UnitCost;
    }

    public void UpdateUnitCost(double newUnitCost)
    {
        if (newUnitCost < 0)
            throw new ArgumentOutOfRangeException(nameof(newUnitCost), "Unit cost cannot be negative.");

        UnitCost = newUnitCost;
        Amount = Qtty * UnitCost;
    }

    public void Close()
    {
        Status = "CLOSED";
    }

    public void Reopen()
    {
        Status = "OPEN";
    }

    public void SetLineNum(string lineNum)
    {
        DomainGuards.AgainstNullOrWhiteSpace(lineNum);
        LineNum = lineNum;
    }

    public void SetOrderId(string id)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        OrderId = id;
    }

    public void SetStatus(string status)
    {
        DomainGuards.AgainstNullOrWhiteSpace(status);
        Status = status;
    }
}
