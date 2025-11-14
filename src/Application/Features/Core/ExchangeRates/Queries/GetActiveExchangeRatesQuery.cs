using MediatR;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRates.Queries;

public record GetActiveExchangeRatesQuery(
    string? BaseCurrencyCode = null,
    string? TargetCurrencyCode = null,
    RateType? RateType = null,
    DateTime? AsOfDate = null)
    : IRequest<Result<IReadOnlyList<ExchangeRateDto>>>;

public class GetActiveExchangeRatesQueryHandler(
    IExchangeRateRepository exchangeRateRepository)
    : IRequestHandler<GetActiveExchangeRatesQuery, Result<IReadOnlyList<ExchangeRateDto>>>
{
    private readonly IExchangeRateRepository _exchangeRateRepository = exchangeRateRepository;

    public async Task<Result<IReadOnlyList<ExchangeRateDto>>> Handle(
        GetActiveExchangeRatesQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate and parse currency parameters if provided
            var (baseCurrency, targetCurrency, validationResult) = ValidateAndParseCurrencies(query);
            if (!validationResult.Success)
                return Result<IReadOnlyList<ExchangeRateDto>>.Failed(validationResult.Message);

            // Validate date if provided
            if (query.AsOfDate.HasValue && query.AsOfDate.Value > DateTime.UtcNow)
            {
                return Result<IReadOnlyList<ExchangeRateDto>>.Failed(
                    "AsOfDate cannot be in the future");
            }

            // Get active exchange rates from repository using Currency objects
            var activeRates = await _exchangeRateRepository.GetAllActiveRatesAsync(
                query.AsOfDate,
                baseCurrency,
                targetCurrency,
                query.RateType);

            if (!activeRates.Any())
            {
                var message = BuildNoRatesMessage(query);
                return Result<IReadOnlyList<ExchangeRateDto>>.Succeeded(
                    [], message);
            }

            // Map to DTOs
            var rateDtos = MapToDtos(activeRates);

            return Result<IReadOnlyList<ExchangeRateDto>>.Succeeded(rateDtos);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<ExchangeRateDto>>.Failed(
                $"An error occurred while retrieving active exchange rates: {ex.Message}");
        }
    }

    private static (Currency? baseCurrency, Currency? targetCurrency, Result validationResult)
        ValidateAndParseCurrencies(GetActiveExchangeRatesQuery query)
    {
        Currency? baseCurrency = null;
        Currency? targetCurrency = null;

        // Validate base currency if provided
        if (!string.IsNullOrWhiteSpace(query.BaseCurrencyCode))
        {
            if (!Currency.TryFromCode(query.BaseCurrencyCode, out var baseCurrencyParsed))
            {
                return (null, null, Result.Failed($"Invalid base currency: {query.BaseCurrencyCode}"));
            }
            baseCurrency = baseCurrencyParsed;
        }

        // Validate target currency if provided
        if (!string.IsNullOrWhiteSpace(query.TargetCurrencyCode))
        {
            if (!Currency.TryFromCode(query.TargetCurrencyCode, out var targetCurrencyParsed))
            {
                return (null, null, Result.Failed($"Invalid target currency: {query.TargetCurrencyCode}"));
            }
            targetCurrency = targetCurrencyParsed;
        }

        // Validate that both currencies are provided if one is provided
        if ((!string.IsNullOrWhiteSpace(query.BaseCurrencyCode) && string.IsNullOrWhiteSpace(query.TargetCurrencyCode)) ||
            (string.IsNullOrWhiteSpace(query.BaseCurrencyCode) && !string.IsNullOrWhiteSpace(query.TargetCurrencyCode)))
        {
            return (null, null, Result.Failed("Both base currency and target currency must be provided together, or neither"));
        }

        // Validate currency pair if both provided
        if (baseCurrency != null && targetCurrency != null && baseCurrency == targetCurrency)
        {
            return (null, null, Result.Failed("Base currency and target currency cannot be the same"));
        }

        return (baseCurrency, targetCurrency, Result.Succeeded());
    }

    private static string BuildNoRatesMessage(GetActiveExchangeRatesQuery query)
    {
        var messageParts = new List<string> { "No active exchange rates found" };

        if (query.AsOfDate.HasValue)
        {
            messageParts.Add($"as of {query.AsOfDate.Value:yyyy-MM-dd HH:mm:ss}");
        }

        if (!string.IsNullOrWhiteSpace(query.BaseCurrencyCode))
        {
            messageParts.Add($"for {query.BaseCurrencyCode}/{query.TargetCurrencyCode}");
        }

        if (query.RateType.HasValue)
        {
            messageParts.Add($"with type {query.RateType}");
        }

        return string.Join(" ", messageParts);
    }

    private static IReadOnlyList<ExchangeRateDto> MapToDtos(IReadOnlyList<ExchangeRate> exchangeRates)
    {
        return exchangeRates.Select(rate => rate.ToDto()).ToList();
    }
}

public static class ExchangeRateMapper
{
    public static ExchangeRateDto ToDto(this ExchangeRate src)
    {
        if (src == null)
            throw new ArgumentNullException(nameof(src));

        return new ExchangeRateDto
        {
            Id = src.Id,
            BaseCurrency = src.BaseCurrency,
            TargetCurrency = src.TargetCurrency,
            MarketRate = src.MarketRate,
            EffectiveRate = src.EffectiveRate,
            Margin = src.Margin,
            Type = src.Type,
            EffectiveFrom = src.EffectiveFrom,
            EffectiveTo = src.EffectiveTo,
            IsActive = src.IsActive,
            Source = src.Source,
            ExchangeRateDescription = src.GetRateDescription(),
            ExchangeRateInverseDescription = src.GetInverseRateDescription(),
            ExchangeRateShortDescription = src.GetRateShortDescription(),
            ExchangeRateInverseShortDescription = src.GetInverseRateShortDescription(),
            ClientGroupName = src.ClientGroup?.Name,
            ClientName = src.Client?.FullName,
            RateTypeDescription = GetRateTypeDescription(src.Type)
        };
    }

    private static string GetRateTypeDescription(RateType type)
    {
        return type switch
        {
            RateType.General => "General Rate",
            RateType.Group => "Group Rate",
            RateType.Individual => "Individual Rate",
            _ => "Unknown Rate Type"
        };
    }
}
