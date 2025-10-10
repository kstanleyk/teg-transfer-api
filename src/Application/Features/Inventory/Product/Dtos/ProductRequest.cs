namespace Transfer.Application.Features.Inventory.Product.Dtos;

public abstract class BaseProductRequest
{
    public Guid PublicId { get; set; }
    public required string Name { get; set; }
    public required string ShortDescription { get; set; }
    public required string Sku { get; set; }
    public required string BarCodeText { get; set; }
    public required string Brand { get; set; }
    public required string Category { get; set; }
    public required string UnitSize { get; set; }
    public required string Packaging { get; set; }
    public required string Status { get; set; }
    public double MinStock { get; set; }
    public double MaxStock { get; set; }
    public double ReorderLev { get; set; }
    public double ReorderQtty { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CreateProductRequest : BaseProductRequest
{
    // Nothing extra, inherits all properties from BaseProductRequest
}

public class EditProductRequest : BaseProductRequest
{
    public string Id { get; set; } = string.Empty;
}

public class ItemValidationCodes
{
    public IEnumerable<string> CategoryCodes { get; set; } = [];
}