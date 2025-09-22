namespace Agrovet.Application.Features.Inventory.Item.Dtos;

public abstract class BaseItemRequest
{
    public Guid? PublicId { get; set; }
    public required string Name { get; set; }
    public required string ShortDescription { get; set; }
    public required string BarCodeText { get; set; }
    public required string Brand { get; set; }
    public required string Category { get; set; }
    public required string Status { get; set; }
    public double MinStock { get; set; }
    public double MaxStock { get; set; }
    public double ReorderLev { get; set; }
    public double ReorderQtty { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CreateItemRequest : BaseItemRequest
{
    // Nothing extra, inherits all properties from BaseItemRequest
}

public class EditItemRequest : BaseItemRequest
{
    public string Id { get; set; } = string.Empty;
}

public class ItemValidationCodes
{
    public IEnumerable<string> CategoryCodes { get; set; } = [];
}