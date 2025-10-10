namespace Transfer.Application.Features.Inventory.Country.Dtos;

public class CountryResponse
{
    public string Id { get; set; } = null!;
    public Guid? PublicId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}

public class CountryCreatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CountryUpdatedResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedOn { get; set; }
}