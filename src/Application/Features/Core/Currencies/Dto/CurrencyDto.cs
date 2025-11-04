namespace TegWallet.Application.Features.Core.Currencies.Dto;

public class CurrencyDto
{
    public string Code { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public decimal DecimalPlaces { get; set; }
}