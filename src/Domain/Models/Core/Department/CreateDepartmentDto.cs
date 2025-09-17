namespace Agrovet.Domain.Models.Core.Department;

public class CreateDepartmentDto
{
    public string Name { get; set; } = null!;
    public string FacultyId { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}