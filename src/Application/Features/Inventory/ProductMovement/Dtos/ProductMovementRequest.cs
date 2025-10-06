namespace Agrovet.Application.Features.Inventory.ProductMovement.Dtos;

public abstract class BaseProductMovementRequest
{
    public string Id { get; set; } = string.Empty;
    public required string LineNum { get; set; }
    public required string Description { get; set; }
    public required string Item { get; set; }
    public DateTime TransDate { get; set; }
    public required string TransTime { get; set; }
    public required string Sense { get; set; } // "IN" / "OUT"
    public double Qtty { get; set; }
    public required string SourceId { get; set; }
    public required string SourceLineNum { get; set; }
    public Guid PublicId { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CreateProductMovementRequest : BaseProductMovementRequest
{
    // Inherits all properties for creating a new movement
}

public class EditProductMovementRequest : BaseProductMovementRequest
{
    // PublicId is already included in the base class for editing
}

public class ProductMovementValidationCodes
{
    public IEnumerable<string> ValidSenses { get; set; } = new[] { "IN", "OUT" };
}