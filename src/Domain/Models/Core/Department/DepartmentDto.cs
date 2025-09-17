namespace Agrovet.Domain.Models.Core.Department;

public class DepartmentDto
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public string? FacultyId { get; set; }
    public DateTime DateCreated { get; set; }
}