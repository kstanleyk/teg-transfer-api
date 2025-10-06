namespace Agrovet.Application.Features.Inventory.OrderType.Dtos;

public abstract class BaseOrderTypeRequest
{
    public required string Name { get; set; }
}

public class CreateOrderTypeRequest : BaseOrderTypeRequest
{
    
}

public class EditOrderTypeRequest : BaseOrderTypeRequest
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
}

public class OrderTypeValidationCodes
{
    public IEnumerable<string> ValidIds { get; set; } = [];
}