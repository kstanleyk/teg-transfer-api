namespace Agrovet.Application.Models.Core.Department;

public class DepartmentDto
{
    public Guid PublicId { get; set; }
    public required string Name { get; set; }
    public DateTime DateCreated { get; set; }
}

public abstract class BaseDepartmentDto
{
    public string Name { get; set; } = null!;
    public string FacultyId { get; set; } = null!;
}

public class EditDepartmentDto: BaseDepartmentDto
{
    public string Id { get; set; } = string.Empty;
}

public class CreateDepartmentDto : BaseDepartmentDto
{

}