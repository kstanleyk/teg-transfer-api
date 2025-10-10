namespace Transfer.Application.Features.Inventory.Product.Dtos;

public class ProductResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public string? Sku { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? SizeInLitters { get; set; }
    public string? Status { get; set; }
    public double? MinStock { get; set; }
    public double? MaxStock { get; set; }
    public double? ReorderLev { get; set; }
    public double? ReorderQtty { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class ProductCreatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public string? ShortDescription { get; set; }
    public string? BarCodeText { get; set; }
    public string? Sku { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? UnitSize { get; set; }
    public string? Status { get; set; }
    public double? MinStock { get; set; }
    public double? MaxStock { get; set; }
    public double? ReorderLev { get; set; }
    public double? ReorderQtty { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class ProductUpdatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public string? ShortDescription { get; set; }
    public string? BarCodeText { get; set; }
    public string? Sku { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? UnitSize { get; set; }
    public string? Status { get; set; }
    public double? MinStock { get; set; }
    public double? MaxStock { get; set; }
    public double? ReorderLev { get; set; }
    public double? ReorderQtty { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class ProductStockBalanceResponse
{
    public string Id { get; set; } = string.Empty;
    public string LineNum { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string PackagingType { get; set; } = string.Empty;
    public double UnitsPerBox { get; set; } = 0;
    public double StockQtty { get; set; } = 0;
    public double UnitCost { get; set; } = 0;
    public double Amount { get; set; } = 0;
}