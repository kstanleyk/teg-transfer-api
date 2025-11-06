using TegWallet.Application.Features.Core.ExchangeRates.Command;
using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Interfaces.Core;

public interface IExchangeRateRepository : IRepository<ExchangeRate, Guid>
{
    Task<IReadOnlyList<ExchangeRate>> GetActiveRatesAsync(Currency baseCurrency, Currency targetCurrency,
        DateTime effectiveDate);

    Task<IReadOnlyList<ExchangeRate>> GetRatesByClientAsync(Guid clientId);

    Task<IReadOnlyList<ExchangeRate>> GetHistoricalRatesAsync(Currency baseCurrency,
        Currency targetCurrency, DateTime from, DateTime to);

    Task<ExchangeRate?> GetByIdAsync(Guid id);

    Task<ExchangeRate?> GetEffectiveRateForClientAsync(
        Guid clientId,
        Guid? clientGroupId,
        Currency baseCurrency,
        Currency targetCurrency,
        DateTime asOfDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExchangeRate>> GetClientAvailableRatesAsync(
        Guid clientId,
        Currency baseCurrency,
        DateTime asOfDate);

    Task<IReadOnlyList<ExchangeRate>> GetExchangeRateHistoryAsync(Currency baseCurrency,
        Currency targetCurrency, DateTime fromDate, DateTime toDate, RateType? type, Guid? clientOrGroupId);

    Task<RepositoryActionResult<ExchangeRate>> CreateGeneralExchangeRateAsync(CreateGeneralExchangeRateParameters parameters);

    Task<RepositoryActionResult<ExchangeRate>> CreateGroupExchangeRateAsync(
        CreateGroupExchangeRateParameters parameters);

    Task<RepositoryActionResult<ExchangeRate>> CreateIndividualExchangeRateAsync(
        CreateIndividualExchangeRateParameters parameters);

    Task<RepositoryActionResult<ExchangeRate>> DeactivateExchangeRateAsync(
        DeactivateExchangeRateParameters parameters);

    Task<RepositoryActionResult<ExchangeRate>> ExtendExchangeRateValidityAsync(
        ExtendExchangeRateValidityParameters parameters);

    Task<RepositoryActionResult<ExchangeRate>> UpdateExchangeRateAsync(
        UpdateExchangeRateParameters parameters);
}