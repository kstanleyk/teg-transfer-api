namespace Agrovet.Application.Features.Inventory.ItemCategory.Dtos;

public class ItemCategoryResponse
{
    public string Id { get; set; } = null!;
    public Guid? PublicId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}

public class ItemCategoryCreatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class ItemCategoryUpdatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedOn { get; set; }
}