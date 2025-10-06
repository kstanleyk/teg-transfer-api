namespace Agrovet.Application.Features.Inventory.OrderStatus.Dtos;

public abstract class BaseOrderStatusRequest
{
    public required string Name { get; set; }
}

public class CreateOrderStatusRequest : BaseOrderStatusRequest
{
    
}

public class EditOrderStatusRequest : BaseOrderStatusRequest
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
}

public class OrderStatusValidationCodes
{
    public IEnumerable<string> ValidIds { get; set; } = [];
}