namespace Agrovet.Application.Models.Core.Department;

public class DepartmentCreatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
}

public class DepartmentUpdatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public string? FacultyId { get; set; }
    public DateTime DateCreated { get; set; }
}