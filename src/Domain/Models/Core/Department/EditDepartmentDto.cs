namespace Agrovet.Domain.Models.Core.Department;

public class EditDepartmentDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = null!;
    public string FacultyId { get; set; } = null!;
}