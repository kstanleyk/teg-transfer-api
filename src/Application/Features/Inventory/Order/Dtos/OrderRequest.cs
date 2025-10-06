namespace Agrovet.Application.Features.Inventory.Order.Dtos;

public abstract class BaseOrderRequest
{
    public string OrderType { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public DateTime TransDate { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CreateOrderRequest : BaseOrderRequest
{
    // For creating new orders
    public List<CreateOrderDetailRequest>? OrderDetails { get; set; } = [];
}

public class EditOrderRequest : BaseOrderRequest
{
    public string Id { get; set; } = string.Empty; // For edit scenarios
    public Guid PublicId { get; set; }
    // For editing existing orders
    public List<EditOrderDetailRequest> OrderDetails { get; set; } = [];
}

public abstract class BaseOrderDetailRequest
{
    public string LineNum { get; set; } = string.Empty;
    public string Item { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public DateTime? ReceiveDate { get; set; }
    public double Qtty { get; set; }
    public double UnitCost { get; set; }
    public double Amount { get; set; }
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
