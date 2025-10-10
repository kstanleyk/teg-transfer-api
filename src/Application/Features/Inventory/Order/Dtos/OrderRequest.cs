using Transfer.Application.Features.Inventory.OrderDetail.Dtos;

namespace Transfer.Application.Features.Inventory.Order.Dtos;

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

public class OrderValidationCodes
{
    public IEnumerable<string> ValidSuppliers { get; set; } = [];
}
