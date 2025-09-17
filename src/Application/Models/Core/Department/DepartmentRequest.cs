namespace Agrovet.Application.Models.Core.Department;

public class DepartmentRequest
{
    public Guid PublicId { get; set; }
    public required string Name { get; set; }
    public DateTime DateCreated { get; set; }
}

public abstract class BaseDepartmentRequest
{
    public string Name { get; set; } = null!;
    public string FacultyId { get; set; } = null!;
    public DateTime DateCreated { get; set; }
}

public class EditDepartmentRequest: BaseDepartmentRequest
{
    public string Id { get; set; } = string.Empty;
}

public class CreateDepartmentRequest : BaseDepartmentRequest
{

}