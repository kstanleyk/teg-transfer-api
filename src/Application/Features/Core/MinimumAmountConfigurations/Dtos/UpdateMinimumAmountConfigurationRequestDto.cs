namespace TegWallet.Application.Features.Core.MinimumAmountConfigurations.Dtos;

public record UpdateMinimumAmountConfigurationRequestDto(
    decimal MinimumAmount,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo = null,
    string UpdatedBy = "SYSTEM");