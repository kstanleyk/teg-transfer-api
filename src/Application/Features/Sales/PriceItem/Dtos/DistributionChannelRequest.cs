namespace Agrovet.Application.Features.Sales.PriceItem.Dtos;

public abstract class BasePriceItemRequest
{
    public string ChannelId { get; set; } = null!;
    public string ItemId { get; set; } = null!;
    public double Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
}

public class CreatePriceItemRequest : BasePriceItemRequest
{
    
}

public class EditPriceItemRequest : BasePriceItemRequest
{
}

public class PriceItemValidationCodes
{
    public IEnumerable<string> ValidIds { get; set; } = [];
}