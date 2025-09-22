namespace Agrovet.Application.Features.Inventory.Order.Dtos;

// Response for full Order details
public class OrderResponse
{
    public string Id { get; set; } = null!;
    public Guid? PublicId { get; set; }
    public string OrderType { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Supplier { get; set; } = null!;
    public DateTime TransDate { get; set; }
    public DateTime CreatedOn { get; set; }

    public List<OrderDetailResponse> OrderDetails { get; set; } = new();
}

// Response for full OrderDetail
public class OrderDetailResponse
{
    public string OrderId { get; set; } = null!;
    public string LineNum { get; set; } = null!;
    public string Item { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Qtty { get; set; }
    public double UnitCost { get; set; }
    public double Amount { get; set; }
    public string Status { get; set; } = null!;
    public DateTime TransDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime? ReceiveDate { get; set; }
    public Guid? PublicId { get; set; }
    public DateTime CreatedOn { get; set; }
}

// Response after creating a new Order
public class OrderCreatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
}

// Response after updating an existing Order
public class OrderUpdatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public DateTime? TransDate { get; set; }
}