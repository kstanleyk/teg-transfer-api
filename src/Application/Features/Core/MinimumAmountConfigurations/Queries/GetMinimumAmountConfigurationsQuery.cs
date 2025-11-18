using MediatR;
using TegWallet.Application.Features.Core.MinimumAmountConfigurations.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.MinimumAmountConfigurations.Queries;

public record GetMinimumAmountConfigurationsQuery(
    string? BaseCurrencyCode = null,
    string? TargetCurrencyCode = null,
    DateTime? AsOfDate = null,
    bool ActiveOnly = true) : IRequest<Result<IReadOnlyList<MinimumAmountConfigurationDto>>>;

public class GetMinimumAmountConfigurationsQueryHandler(
    IMinimumAmountConfigurationRepository minimumAmountConfigurationRepository)
    : IRequestHandler<GetMinimumAmountConfigurationsQuery, Result<IReadOnlyList<MinimumAmountConfigurationDto>>>
{
    public async Task<Result<IReadOnlyList<MinimumAmountConfigurationDto>>> Handle(
        GetMinimumAmountConfigurationsQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            Currency? baseCurrency = null;
            Currency? targetCurrency = null;

            if (!string.IsNullOrWhiteSpace(query.BaseCurrencyCode))
            {
                if (!Currency.TryFromCode(query.BaseCurrencyCode, out var baseCurrencyParsed))
                {
                    return Result<IReadOnlyList<MinimumAmountConfigurationDto>>.Failed($"Invalid base currency: {query.BaseCurrencyCode}");
                }
                baseCurrency = baseCurrencyParsed;
            }

            if (!string.IsNullOrWhiteSpace(query.TargetCurrencyCode))
            {
                if (!Currency.TryFromCode(query.TargetCurrencyCode, out var targetCurrencyParsed))
                {
                    return Result<IReadOnlyList<MinimumAmountConfigurationDto>>.Failed($"Invalid target currency: {query.TargetCurrencyCode}");
                }
                targetCurrency = targetCurrencyParsed;
            }

            var configurations = await minimumAmountConfigurationRepository.GetActiveConfigurationsAsync(
                baseCurrency, targetCurrency, query.AsOfDate);

            if (!configurations.Any())
            {
                return Result<IReadOnlyList<MinimumAmountConfigurationDto>>.Succeeded(
                    Array.Empty<MinimumAmountConfigurationDto>(),
                    "No minimum amount configurations found");
            }

            var dtos = configurations.Select(config => new MinimumAmountConfigurationDto(
                config.Id,
                config.BaseCurrency.Code,
                config.TargetCurrency.Code,
                config.MinimumAmount,
                config.IsActive,
                config.EffectiveFrom,
                config.EffectiveTo,
                config.CreatedBy,
                config.CreatedAt
            )).ToList();

            return Result<IReadOnlyList<MinimumAmountConfigurationDto>>.Succeeded(dtos);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<MinimumAmountConfigurationDto>>.Failed(
                $"An error occurred while retrieving minimum amount configurations: {ex.Message}");
        }
    }
}

// Query for getting by ID
public record GetMinimumAmountConfigurationByIdQuery(Guid ConfigurationId)
    : IRequest<Result<MinimumAmountConfigurationDto>>;

public class GetMinimumAmountConfigurationByIdQueryHandler(
    IMinimumAmountConfigurationRepository minimumAmountConfigurationRepository)
    : IRequestHandler<GetMinimumAmountConfigurationByIdQuery, Result<MinimumAmountConfigurationDto>>
{
    public async Task<Result<MinimumAmountConfigurationDto>> Handle(
        GetMinimumAmountConfigurationByIdQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var configuration = await minimumAmountConfigurationRepository.GetByIdAsync(query.ConfigurationId);

            if (configuration == null)
            {
                return Result<MinimumAmountConfigurationDto>.Failed("Minimum amount configuration not found");
            }

            var dto = new MinimumAmountConfigurationDto(
                configuration.Id,
                configuration.BaseCurrency.Code,
                configuration.TargetCurrency.Code,
                configuration.MinimumAmount,
                configuration.IsActive,
                configuration.EffectiveFrom,
                configuration.EffectiveTo,
                configuration.CreatedBy,
                configuration.CreatedAt
            );

            return Result<MinimumAmountConfigurationDto>.Succeeded(dto);
        }
        catch (Exception ex)
        {
            return Result<MinimumAmountConfigurationDto>.Failed(
                $"An error occurred while retrieving the minimum amount configuration: {ex.Message}");
        }
    }
}

// Query for getting applicable configuration
public record GetApplicableMinimumAmountConfigurationQuery(
    string BaseCurrencyCode,
    string TargetCurrencyCode,
    DateTime? AsOfDate = null) : IRequest<Result<MinimumAmountConfigurationDto>>;

public class GetApplicableMinimumAmountConfigurationQueryHandler(
    IMinimumAmountConfigurationRepository minimumAmountConfigurationRepository)
    : IRequestHandler<GetApplicableMinimumAmountConfigurationQuery, Result<MinimumAmountConfigurationDto>>
{
    public async Task<Result<MinimumAmountConfigurationDto>> Handle(
        GetApplicableMinimumAmountConfigurationQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!Currency.TryFromCode(query.BaseCurrencyCode, out var baseCurrency))
            {
                return Result<MinimumAmountConfigurationDto>.Failed($"Invalid base currency: {query.BaseCurrencyCode}");
            }

            if (!Currency.TryFromCode(query.TargetCurrencyCode, out var targetCurrency))
            {
                return Result<MinimumAmountConfigurationDto>.Failed($"Invalid target currency: {query.TargetCurrencyCode}");
            }

            var asOfDate = query.AsOfDate ?? DateTime.UtcNow;
            var configuration = await minimumAmountConfigurationRepository.GetApplicableMinimumAmountAsync(
                baseCurrency, targetCurrency, asOfDate);

            if (configuration == null)
            {
                return Result<MinimumAmountConfigurationDto>.Succeeded(
                    null!,
                    $"No active minimum amount configuration found for {query.BaseCurrencyCode}/{query.TargetCurrencyCode} as of {asOfDate:yyyy-MM-dd}");
            }

            var dto = new MinimumAmountConfigurationDto(
                configuration.Id,
                configuration.BaseCurrency.Code,
                configuration.TargetCurrency.Code,
                configuration.MinimumAmount,
                configuration.IsActive,
                configuration.EffectiveFrom,
                configuration.EffectiveTo,
                configuration.CreatedBy,
                configuration.CreatedAt
            );

            return Result<MinimumAmountConfigurationDto>.Succeeded(dto);
        }
        catch (Exception ex)
        {
            return Result<MinimumAmountConfigurationDto>.Failed(
                $"An error occurred while retrieving the applicable minimum amount configuration: {ex.Message}");
        }
    }
}