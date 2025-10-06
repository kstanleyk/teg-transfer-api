namespace Agrovet.Application.Features.Sales.PriceItem.Dtos;

public class PriceItemResponse
{
    public string ChannelId { get; set; } = null!;
    public string ItemId { get; set; } = null!;
    public double Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
}

public class PriceItemCreatedResponse
{
    public string ChannelId { get; set; } = null!;
    public string ItemId { get; set; } = null!;
    public double Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
}

public class PriceItemUpdatedResponse
{
    public string ChannelId { get; set; } = null!;
    public string ItemId { get; set; } = null!;
    public double Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
}