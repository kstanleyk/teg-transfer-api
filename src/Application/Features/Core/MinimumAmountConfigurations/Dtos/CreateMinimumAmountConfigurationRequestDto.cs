namespace TegWallet.Application.Features.Core.MinimumAmountConfigurations.Dtos;

public record CreateMinimumAmountConfigurationRequestDto(
    string BaseCurrencyCode,
    string TargetCurrencyCode,
    decimal MinimumAmount,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo = null,
    string CreatedBy = "SYSTEM");