namespace Agrovet.Application.Models.Core.EducationLevel;

public class EditEducationLevelRequest
{
    public Guid PublicId { get; set; } 
    public string Name { get; set; } = null!;
}