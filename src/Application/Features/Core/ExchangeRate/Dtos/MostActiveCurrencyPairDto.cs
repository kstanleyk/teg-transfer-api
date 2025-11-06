namespace TegWallet.Application.Features.Core.ExchangeRate.Dtos;

public class MostActiveCurrencyPairDto
{
    public string CurrencyPair { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalVolume { get; set; }
}