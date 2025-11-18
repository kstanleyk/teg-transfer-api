namespace TegWallet.Application.Features.Core.ExchangeRates.Queries;

// Request DTO for bulk operation
public record GetBulkApplicableExchangeRatesWithTiersRequest
{
    public List<Guid> ClientIds { get; init; } = new();
    public string BaseCurrencyCode { get; init; } = string.Empty;
    public string TargetCurrencyCode { get; init; } = string.Empty;
    public decimal TransactionAmount { get; init; }
    public DateTime? AsOfDate { get; init; }
}