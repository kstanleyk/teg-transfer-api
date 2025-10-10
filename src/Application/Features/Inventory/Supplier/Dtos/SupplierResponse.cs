namespace Transfer.Application.Features.Inventory.Supplier.Dtos;

public class SupplierResponse
{
    public string Id { get; set; } = null!;
    public Guid PublicId { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string ContactPerson { get; set; } = null!;
}

public class SupplierCreatedResponse
{
    public string Id { get; set; } = null!;
    public Guid PublicId { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string ContactPerson { get; set; } = null!;
}

public class SupplierUpdatedResponse
{
    public string Id { get; set; } = null!;
    public Guid PublicId { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string ContactPerson { get; set; } = null!;
}