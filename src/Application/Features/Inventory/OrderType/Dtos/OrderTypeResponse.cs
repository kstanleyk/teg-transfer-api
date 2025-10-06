namespace Agrovet.Application.Features.Inventory.OrderType.Dtos;

public class OrderTypeResponse
{
    public string Id { get; set; } = null!;
    public Guid? PublicId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}

public class OrderTypeCreatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class OrderTypeUpdatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedOn { get; set; }
}