namespace Agrovet.Application.Features.Inventory.Supplier.Dtos;

public abstract class BaseSupplierRequest
{
    public string Id { get; set; } = null!;
    public Guid PublicId { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string ContactPerson { get; set; } = null!;
}

public class CreateSupplierRequest : BaseSupplierRequest
{
    
}

public class EditSupplierRequest : BaseSupplierRequest
{

}

public class SupplierValidationCodes
{
    public IEnumerable<string> ValidIds { get; set; } = [];
}