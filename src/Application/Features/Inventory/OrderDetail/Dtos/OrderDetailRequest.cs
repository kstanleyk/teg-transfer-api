namespace Transfer.Application.Features.Inventory.OrderDetail.Dtos;

public abstract class BaseOrderDetailRequest
{
    public string LineNum { get; set; } = string.Empty;
    public string Item { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string PackagingType { get; set; } = string.Empty;
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