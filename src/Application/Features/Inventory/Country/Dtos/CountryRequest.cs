namespace Transfer.Application.Features.Inventory.Country.Dtos;

public abstract class BaseCountryRequest
{
    public required string Name { get; set; }
}

public class CreateCountryRequest : BaseCountryRequest
{
    
}

public class EditCountryRequest : BaseCountryRequest
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
}

public class CountryValidationCodes
{
    public IEnumerable<string> ValidIds { get; set; } = [];
}