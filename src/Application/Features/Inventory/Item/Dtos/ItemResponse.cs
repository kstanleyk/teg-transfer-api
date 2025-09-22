namespace Agrovet.Application.Features.Inventory.Item.Dtos;

public class ItemResponse
{
    public string Id { get; set; } = null!;
    public Guid? PublicId { get; set; }
    public string Name { get; set; } = null!;
    public string ShortDescription { get; set; } = null!;
    public string BarCodeText { get; set; } = null!;
    public string Brand { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Status { get; set; } = null!;
    public double MinStock { get; set; }
    public double MaxStock { get; set; }
    public double ReorderLev { get; set; }
    public double ReorderQtty { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class ItemCreatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
}

public class ItemUpdatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public string? ShortDescription { get; set; }
    public string? BarCodeText { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public double? MinStock { get; set; }
    public double? MaxStock { get; set; }
    public double? ReorderLev { get; set; }
    public double? ReorderQtty { get; set; }
    public DateTime CreatedOn { get; set; }
}