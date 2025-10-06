namespace Agrovet.Application.Features.Inventory.ProductCategory.Dtos;

public class ProductCategoryResponse
{
    public string Id { get; set; } = null!;
    public Guid? PublicId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}

public class ProductCategoryCreatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class ProductCategoryUpdatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedOn { get; set; }
}