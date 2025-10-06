namespace Agrovet.Application.Features.Inventory.ProductCategory.Dtos;

public abstract class BaseProductCategoryRequest
{
    public required string Name { get; set; }
}

public class CreateProductCategoryRequest : BaseProductCategoryRequest
{
    
}

public class EditProductCategoryRequest : BaseProductCategoryRequest
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
}

public class ItemCategoryValidationCodes
{
    public IEnumerable<string> ValidIds { get; set; } = [];
}