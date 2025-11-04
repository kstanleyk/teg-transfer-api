namespace TegWallet.Application.Features.Core.Wallets.Dto;

public class MoneyDto
{
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;

    // Parameterless constructor for AutoMapper
    public MoneyDto() { }

    // Optional: Constructor for manual creation
    public MoneyDto(decimal amount, string currencyCode, string currencySymbol)
    {
        Amount = amount;
        CurrencyCode = currencyCode;
        CurrencySymbol = currencySymbol;
    }
}