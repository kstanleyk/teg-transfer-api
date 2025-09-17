namespace Agrovet.Domain.Models.Core.Country;

public class EditCountryRequest
{
    public Guid PublicId { get; set; } 
    public string Name { get; set; } = null!;
}