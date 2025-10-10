namespace Transfer.Application.Features.Inventory.OrderDetail.Dtos;

// Response for full OrderDetail
public class OrderDetailResponse
{
    public string Id { get; set; } = null!;
    public string LineNum { get; set; } = null!;
    public string Item { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Qtty { get; set; }
    public double UnitCost { get; set; }
    public double Amount { get; set; }
    public string Status { get; set; } = null!;
    public DateTime TransDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public Guid? PublicId { get; set; }
    public DateTime CreatedOn { get; set; }

    // Flattened PackagingType properties
    public string PackagingType { get; set; } = null!;
    public string PackagingDisplayName { get; set; } = null!;
    public int UnitsPerBox { get; set; }

    // Flattened BottlingType properties
    public decimal BottlingSizeInLiters { get; set; }
    public string BottlingDisplayName { get; set; } = null!;

    public string BatchNumber { get; set; } = null!;
}