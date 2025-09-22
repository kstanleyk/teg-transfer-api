namespace Agrovet.Application.Features.Inventory.ItemMovement.Dtos;

public class ItemMovementResponse
{
    public string Id { get; set; } = null!;
    public Guid? PublicId { get; set; }
    public string LineNum { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Item { get; set; } = null!;
    public DateTime TransDate { get; set; }
    public string TransTime { get; set; } = null!;
    public string Sense { get; set; } = null!;
    public double Qtty { get; set; }
    public string SourceId { get; set; } = null!;
    public string SourceLineNum { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}

public class ItemMovementCreatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
}

public class ItemMovementUpdatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Description { get; set; }
    public string? Item { get; set; }
    public double? Qtty { get; set; }
    public string? Sense { get; set; }
    public DateTime? TransDate { get; set; }
}