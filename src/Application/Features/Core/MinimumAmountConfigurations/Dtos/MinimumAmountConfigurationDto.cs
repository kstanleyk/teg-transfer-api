namespace TegWallet.Application.Features.Core.MinimumAmountConfigurations.Dtos;

public record MinimumAmountConfigurationDto(
    Guid Id,
    string BaseCurrencyCode,
    string TargetCurrencyCode,
    decimal MinimumAmount,
    bool IsActive,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    string CreatedBy,
    DateTime CreatedAt);