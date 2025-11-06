namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public class MostActiveCurrencyPairDto
{
    public string CurrencyPair { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalVolume { get; set; }
}