using Transfer.Domain.Abstractions;

namespace Transfer.Domain.Entity.Inventory;

public class OrderDetail : Entity<string>
{
    public string LineNum { get; private set; } = null!;
    public string Item { get; private set; } = null!;
    public string BatchNumber { get; private set; } = null!;
    public DateTime? ExpiryDate { get; private set; } = null;
    public string Description { get; private set; } = null!;
    public double Qtty { get; private set; }
    public double UnitCost { get; private set; }
    public double Amount { get; private set; }
    public string Status { get; private set; } = null!;
    public DateTime TransDate { get; private set; } = DateTime.UtcNow;
    public PackagingType PackagingType { get; private set; } = null!;

    // Navigation back to Order (aggregate root)
    public Order Order { get; private set; } = null!;

    protected OrderDetail() { }

    public static OrderDetail Create(
        string item,
        string batchNumber,
        double qtty,
        double unitCost,
        PackagingType packagingType,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(item);
        DomainGuards.AgainstNullOrWhiteSpace(batchNumber);

        if (qtty <= 0)
            throw new ArgumentOutOfRangeException(nameof(qtty), "Quantity must be greater than zero.");

        if (unitCost < 0)
            throw new ArgumentOutOfRangeException(nameof(unitCost), "Unit cost cannot be negative.");

        return new OrderDetail
        {
            Item = item,
            BatchNumber = batchNumber,
            Description = item,
            Qtty = qtty,
            UnitCost = unitCost,
            Amount = qtty * unitCost,
            PackagingType = packagingType ?? throw new ArgumentNullException(nameof(packagingType)),
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    // Existing methods...

    public void UpdatePackagingType(PackagingType newPackagingType)
    {
        PackagingType = newPackagingType ?? throw new ArgumentNullException(nameof(newPackagingType));
    }

    // Unboxing functionality
    public bool CanUnbox()
    {
        return PackagingType.IsCarton;
    }

    public OrderDetail UnboxToUnits()
    {
        if (!CanUnbox())
            throw new InvalidOperationException($"Cannot unbox order detail with packaging type {PackagingType.Id}. It is not a carton.");

        var unitPackaging = PackagingType.GetUnitEquivalent();
        var unitsQuantity = Qtty * PackagingType.UnitsPerBox;

        // Calculate unit cost (cost per unit instead of cost per carton)
        var unitCost = UnitCost / PackagingType.UnitsPerBox;

        var unitOrderDetail = Create(
            item: Item,
            batchNumber:BatchNumber,
            qtty: unitsQuantity,
            unitCost: unitCost,
            packagingType: unitPackaging,
            createdOn: CreatedOn
        );

        // Copy relevant properties
        unitOrderDetail.SetId(GenerateUnitDetailId());
        unitOrderDetail.SetLineNum(LineNum); // You might want a different line numbering strategy
        unitOrderDetail.SetOrderId(Id);
        unitOrderDetail.SetStatus(Status);
        unitOrderDetail.Description = Description;
        unitOrderDetail.ExpiryDate = ExpiryDate;
        unitOrderDetail.TransDate = TransDate;

        return unitOrderDetail;
    }

    public bool CanBoxTo(PackagingType targetCartonType)
    {
        if (PackagingType.IsCarton) return false;

        var cartonEquivalent = PackagingType.GetCartonEquivalent();
        return cartonEquivalent?.Id == targetCartonType.Id &&
               Qtty % targetCartonType.UnitsPerBox == 0;
    }

    public OrderDetail BoxToCarton(PackagingType targetCartonType)
    {
        if (!CanBoxTo(targetCartonType))
            throw new InvalidOperationException($"Cannot box order detail to {targetCartonType.Id}. Invalid packaging type or quantity.");

        var cartonsQuantity = Qtty / targetCartonType.UnitsPerBox;
        var cartonUnitCost = UnitCost * targetCartonType.UnitsPerBox;

        var cartonOrderDetail = Create(
            item: Item,
            batchNumber: BatchNumber,
            qtty: cartonsQuantity,
            unitCost: cartonUnitCost,
            packagingType: targetCartonType,
            createdOn: CreatedOn
        );

        // Copy relevant properties
        cartonOrderDetail.SetId(GenerateCartonDetailId());
        cartonOrderDetail.SetLineNum(LineNum);
        cartonOrderDetail.SetOrderId(Id);
        cartonOrderDetail.SetStatus(Status);
        cartonOrderDetail.Description = Description;
        cartonOrderDetail.ExpiryDate = ExpiryDate;
        cartonOrderDetail.TransDate = TransDate;

        return cartonOrderDetail;
    }

    private string GenerateUnitDetailId()
    {
        // Generate a new ID for the unboxed item
        return $"{Id}-UNBOXED-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    private string GenerateCartonDetailId()
    {
        // Generate a new ID for the boxed item
        return $"{Id}-BOXED-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    // Helper method to check if this detail represents unboxed items
    public bool IsUnboxedFrom(OrderDetail cartonDetail)
    {
        return Id.StartsWith($"{cartonDetail.Id}-UNBOXED-");
    }

    // Helper method to check if this detail represents boxed items
    public bool IsBoxedFrom(OrderDetail unitDetail)
    {
        return Id.StartsWith($"{unitDetail.Id}-BOXED-");
    }

    public void SetLineNum(string lineNum)
    {
        DomainGuards.AgainstNullOrWhiteSpace(lineNum);
        LineNum = lineNum;
    }

    public void SetOrderId(string id)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        Id = id;
    }

    public void SetStatus(string status)
    {
        DomainGuards.AgainstNullOrWhiteSpace(status);
        Status = status;
    }
}

//public class OrderDetail : Entity<string>
//{
//    public string LineNum { get; private set; } = null!;
//    public string Item { get; private set; } = null!;
//    public string BatchNumber { get; private set; } = null!;
//    public DateTime? ExpiryDate { get; private set; } = null;
//    public string Description { get; private set; } = null!;
//    public double Qtty { get; private set; }
//    public double UnitCost { get; private set; }
//    public double Amount { get; private set; }
//    public string Status { get; private set; } = null!;
//    public DateTime TransDate { get; private set; } = DateTime.UtcNow;

//    // New PackagingType property
//    public PackagingType PackagingType { get; private set; } = null!;

//    // Navigation back to Order (aggregate root)
//    public Order Order { get; private set; } = null!;

//    protected OrderDetail() { }

//    public static OrderDetail Create(
//        string item,
//        string batchNumber,
//        double qtty,
//        double unitCost,
//        PackagingType packagingType,
//        DateTime? createdOn = null)
//    {
//        DomainGuards.AgainstNullOrWhiteSpace(item);
//        DomainGuards.AgainstNullOrWhiteSpace(batchNumber);

//        if (qtty <= 0)
//            throw new ArgumentOutOfRangeException(nameof(qtty), "Quantity must be greater than zero.");

//        if (unitCost < 0)
//            throw new ArgumentOutOfRangeException(nameof(unitCost), "Unit cost cannot be negative.");

//        return new OrderDetail
//        {
//            Item = item,
//            BatchNumber = batchNumber,
//            Qtty = qtty,
//            UnitCost = unitCost,
//            Amount = qtty * unitCost,
//            PackagingType = packagingType ?? throw new ArgumentNullException(nameof(packagingType)),
//            CreatedOn = createdOn ?? DateTime.UtcNow
//        };
//    }

//    public void UpdateQuantity(double newQtty)
//    {
//        if (newQtty <= 0)
//            throw new ArgumentOutOfRangeException(nameof(newQtty), "Quantity must be greater than zero.");

//        Qtty = newQtty;
//        Amount = Qtty * UnitCost;
//    }

//    public void UpdateUnitCost(double newUnitCost)
//    {
//        if (newUnitCost < 0)
//            throw new ArgumentOutOfRangeException(nameof(newUnitCost), "Unit cost cannot be negative.");

//        UnitCost = newUnitCost;
//        Amount = Qtty * UnitCost;
//    }

//    public void UpdatePackagingType(PackagingType newPackagingType)
//    {
//        PackagingType = newPackagingType ?? throw new ArgumentNullException(nameof(newPackagingType));
//    }

//    public void SetLineNum(string lineNum)
//    {
//        DomainGuards.AgainstNullOrWhiteSpace(lineNum);
//        LineNum = lineNum;
//    }

//    public void SetOrderId(string id)
//    {
//        DomainGuards.AgainstNullOrWhiteSpace(id);
//        Id = id;
//    }

//    public void SetStatus(string status)
//    {
//        DomainGuards.AgainstNullOrWhiteSpace(status);
//        Status = status;
//    }
//}
