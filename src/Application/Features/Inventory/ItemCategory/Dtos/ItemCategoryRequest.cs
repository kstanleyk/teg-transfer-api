namespace Agrovet.Application.Features.Inventory.ItemCategory.Dtos;

public abstract class BaseItemCategoryRequest
{
    public required string Name { get; set; }
}

public class CreateItemCategoryRequest : BaseItemCategoryRequest
{
    
}

public class EditItemCategoryRequest : BaseItemCategoryRequest
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
}

public class ItemCategoryValidationCodes
{
    public IEnumerable<string> ValidIds { get; set; } = [];
}