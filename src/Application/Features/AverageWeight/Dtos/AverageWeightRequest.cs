namespace Agrovet.Application.Features.AverageWeight.Dtos;

public abstract class BaseAverageWeightRequest
{
    public Guid PublicId { get; set; }
    public required string Estate { get; set; }
    public required string Block { get; set; }
    public double Weight { get; set; }
    public DateTime EffectiveDate { get; set; }
    public required string Status { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class EditAverageWeightRequest: BaseAverageWeightRequest
{
    public string Id { get; set; } = string.Empty;
}

public class CreateAverageWeightRequest : BaseAverageWeightRequest
{

}

public class AverageWeightValidationCodes
{
    public required IEnumerable<string> EstateCodes { get; set; }
}