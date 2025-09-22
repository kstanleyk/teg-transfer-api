namespace Agrovet.Application.Features.Inventory.Order.Dtos;

public abstract class BaseOrderRequest
{
    public Guid PublicId { get; set; }
    public required string OrderType { get; set; }
    public DateTime OrderDate { get; set; }
    public required string Status { get; set; }
    public required string Description { get; set; }
    public required string Supplier { get; set; }
    public DateTime TransDate { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CreateOrderRequest : BaseOrderRequest
{
    // For creating new orders
    public List<CreateOrderDetailRequest> OrderDetails { get; set; } = [];
}

public class EditOrderRequest : BaseOrderRequest
{
    public string Id { get; set; } = string.Empty; // For edit scenarios
    // For editing existing orders
    public List<EditOrderDetailRequest> OrderDetails { get; set; } = [];
}

public abstract class BaseOrderDetailRequest
{
    public string LineNum { get; set; } = string.Empty;
    public required string Item { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public required string Description { get; set; }
    public DateTime? ReceiveDate { get; set; }
    public double Qtty { get; set; }
    public double UnitCost { get; set; }
    public double Amount { get; set; }
    public required string Status { get; set; }
    public DateTime TransDate { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CreateOrderDetailRequest : BaseOrderDetailRequest
{
    // OrderId will be set automatically when adding to an Order
}

public class EditOrderDetailRequest : BaseOrderDetailRequest
{
    // OrderId and LineNum identify the detail to update
}

public class OrderValidationCodes
{
    public IEnumerable<string> ValidSuppliers { get; set; } = [];
}
