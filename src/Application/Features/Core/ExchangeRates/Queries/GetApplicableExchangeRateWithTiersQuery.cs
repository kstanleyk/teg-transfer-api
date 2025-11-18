using MediatR;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRates.Queries;

public record GetApplicableExchangeRateWithTiersQuery(
    Guid? ClientId,
    Guid? ClientGroupId,
    string BaseCurrencyCode,
    string TargetCurrencyCode,
    decimal TransactionAmount, // Target currency amount
    DateTime? AsOfDate = null) : IRequest<Result<ExchangeRateApplicationDto>>;

public class GetApplicableExchangeRateWithTiersQueryHandler(
    IExchangeRateRepository exchangeRateRepository)
    : IRequestHandler<GetApplicableExchangeRateWithTiersQuery, Result<ExchangeRateApplicationDto>>
{
    public async Task<Result<ExchangeRateApplicationDto>> Handle(
        GetApplicableExchangeRateWithTiersQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            // Parse currencies
            if (!Currency.TryFromCode(query.BaseCurrencyCode, out var baseCurrency))
            {
                return Result<ExchangeRateApplicationDto>.Failed($"Invalid base currency: {query.BaseCurrencyCode}");
            }

            if (!Currency.TryFromCode(query.TargetCurrencyCode, out var targetCurrency))
            {
                return Result<ExchangeRateApplicationDto>.Failed($"Invalid target currency: {query.TargetCurrencyCode}");
            }

            var asOfDate = query.AsOfDate ?? DateTime.UtcNow;

            // Get applicable rate with tiered logic
            var applicationResult = await exchangeRateRepository.GetApplicableRateWithTiersAsync(
                query.ClientId,
                query.ClientGroupId,
                baseCurrency,
                targetCurrency,
                query.TransactionAmount,
                asOfDate);

            if (applicationResult.ExchangeRate == null && applicationResult.AppliedTier == null)
            {
                return Result<ExchangeRateApplicationDto>.Failed(
                    "No applicable exchange rate found for the given parameters.");
            }

            // Map to DTO
            var dto = MapToDto(applicationResult);

            return Result<ExchangeRateApplicationDto>.Succeeded(dto);
        }
        catch (Exception ex)
        {
            return Result<ExchangeRateApplicationDto>.Failed(
                $"An error occurred while retrieving the applicable exchange rate: {ex.Message}");
        }
    }

    private static ExchangeRateApplicationDto MapToDto(ExchangeRateApplicationResult result)
    {
        return new ExchangeRateApplicationDto
        {
            ExchangeRate = result.ExchangeRate != null ? MapExchangeRateToDto(result.ExchangeRate) : null,
            AppliedTier = result.AppliedTier != null ? new ExchangeRateTierDto
            {
                Id = result.AppliedTier.Id,
                MinAmount = result.AppliedTier.MinAmount,
                MaxAmount = result.AppliedTier.MaxAmount,
                Rate = result.AppliedTier.Rate,
                Margin = result.AppliedTier.Margin,
                CreatedBy = result.AppliedTier.CreatedBy,
                CreatedAt = result.AppliedTier.CreatedAt
            } : null,

            RateType = result.IsTieredRate ? "Tiered" : "Hierarchical",
            IsTieredRate = result.IsTieredRate,
            EffectiveRate = result.EffectiveRate,
            EffectiveMargin = result.EffectiveMargin,
            MinimumAmount = result.MinimumAmount
        };
    }

    private static ExchangeRateDto MapExchangeRateToDto(ExchangeRate exchangeRate)
    {
        var dto = new ExchangeRateDto
        {
            Id = exchangeRate.Id,
            BaseCurrency = exchangeRate.BaseCurrency,
            TargetCurrency = exchangeRate.TargetCurrency,
            MarketRate = exchangeRate.MarketRate, // This should be calculated in the domain
            EffectiveRate = exchangeRate.EffectiveRate, // This should be calculated in the domain
            Margin = exchangeRate.Margin,
            Type = exchangeRate.Type,
            EffectiveFrom = exchangeRate.EffectiveFrom,
            EffectiveTo = exchangeRate.EffectiveTo,
            IsActive = exchangeRate.IsActive,
            Source = exchangeRate.Source,
            ClientGroupName = exchangeRate.ClientGroup?.Name,
            ClientName = exchangeRate.Client?.FullName,
            RateTypeDescription = GetRateTypeDescription(exchangeRate.Type)
        };

        // Set calculated description properties
        SetDescriptionProperties(dto, exchangeRate);

        return dto;
    }

    private static string GetRateTypeDescription(RateType rateType)
    {
        return rateType switch
        {
            RateType.General => "General Rate (Applies to all clients)",
            RateType.Group => "Group Rate (Applies to client group)",
            RateType.Individual => "Individual Rate (Client-specific)",
            _ => "Unknown Rate Type"
        };
    }

    private static void SetDescriptionProperties(ExchangeRateDto dto, ExchangeRate exchangeRate)
    {
        // Set the main description
        dto.ExchangeRateDescription = $"{dto.BaseCurrency.Code} to {dto.TargetCurrency.Code} - {dto.RateTypeDescription}";

        // Set inverse description
        dto.ExchangeRateInverseDescription = $"{dto.TargetCurrency.Code} to {dto.BaseCurrency.Code} - {dto.RateTypeDescription}";

        // Set short description
        dto.ExchangeRateShortDescription = $"{dto.BaseCurrency.Code}/{dto.TargetCurrency.Code} - {dto.Type}";

        // Set inverse short description
        dto.ExchangeRateInverseShortDescription = $"{dto.TargetCurrency.Code}/{dto.BaseCurrency.Code} - {dto.Type}";

        // Add client/group specifics if applicable
        if (exchangeRate.Type == RateType.Individual && !string.IsNullOrEmpty(dto.ClientName))
        {
            dto.ExchangeRateDescription += $" - {dto.ClientName}";
            dto.ExchangeRateShortDescription += $" - {dto.ClientName}";
        }
        else if (exchangeRate.Type == RateType.Group && !string.IsNullOrEmpty(dto.ClientGroupName))
        {
            dto.ExchangeRateDescription += $" - {dto.ClientGroupName}";
            dto.ExchangeRateShortDescription += $" - {dto.ClientGroupName}";
        }
    }
}

public class ExchangeRateApplicationResult
{
    public ExchangeRate? ExchangeRate { get; set; }
    public ExchangeRateTier? AppliedTier { get; set; }
    public RateType RateType { get; set; }
    public bool IsTieredRate { get; set; }
    public decimal EffectiveRate { get; set; }
    public decimal EffectiveMargin { get; set; }
    public decimal MinimumAmount { get; set; }
}

public class RepositoryException : Exception
{
    public RepositoryException(string message) : base(message) { }
    public RepositoryException(string message, Exception innerException) : base(message, innerException) { }
}