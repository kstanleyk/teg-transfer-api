namespace Transfer.Application.Features.Inventory.Warehouse.Dtos;

public abstract class BaseWarehouseRequest
{
    // Warehouse properties
    public string Name { get; set; } = null!;

    // Address properties - flattened
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = null!;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = null!;
    public string ZipCode { get; set; } = string.Empty;
    public string Quarter { get; set; } = string.Empty;
    public string Landmark { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
}

public class CreateWarehouseRequest : BaseWarehouseRequest
{
    
}

public class EditWarehouseRequest : BaseWarehouseRequest
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
}

public class WarehouseValidationCodes
{
    public IEnumerable<string> ValidIds { get; set; } = [];
}