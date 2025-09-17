namespace Agrovet.Application.Features.Departments;

public class DepartmentDto
{
    public Guid PublicId { get; set; }
    public required string Name { get; set; }
    public DateTime DateCreated { get; set; }
}