namespace Agrovet.Application.Features.Inventory.ItemCategory.Dtos;

public abstract class BaseItemCategoryRequest
{
    public Guid? PublicId { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CreateItemCategoryRequest : BaseItemCategoryRequest
{
    // Inherits all properties from BaseItemCategoryRequest
}

public class EditItemCategoryRequest : BaseItemCategoryRequest
{
    public string Id { get; set; } = string.Empty;
}

public class ItemCategoryValidationCodes
{
    public IEnumerable<string> ValidIds { get; set; } = [];
}