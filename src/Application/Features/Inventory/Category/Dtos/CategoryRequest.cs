namespace Transfer.Application.Features.Inventory.Category.Dtos;

public abstract class BaseCategoryRequest
{
    public required string Name { get; set; }
}

public class CreateCategoryRequest : BaseCategoryRequest
{
    
}

public class EditCategoryRequest : BaseCategoryRequest
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
}

public class ItemCategoryValidationCodes
{
    public IEnumerable<string> ValidIds { get; set; } = [];
}