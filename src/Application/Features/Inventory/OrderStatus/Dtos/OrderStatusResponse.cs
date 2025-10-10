namespace Transfer.Application.Features.Inventory.OrderStatus.Dtos;

public class OrderStatusResponse
{
    public string Id { get; set; } = null!;
    public Guid? PublicId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}

public class OrderStatusCreatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class OrderStatusUpdatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedOn { get; set; }
}