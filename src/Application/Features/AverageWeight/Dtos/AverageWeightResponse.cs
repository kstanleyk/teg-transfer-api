namespace Agrovet.Application.Features.AverageWeight.Dtos;
public class AverageWeightResponse
{
    public string Id { get; set; } = null!;
    public Guid? PublicId { get; set; }
    public string Estate { get; set; } = null!;
    public string Block { get; set; } = null!;
    public double Weight { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}

public class AverageWeightCreatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
}

public class AverageWeightUpdatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public string? FacultyId { get; set; }
    public DateTime DateCreated { get; set; }
}