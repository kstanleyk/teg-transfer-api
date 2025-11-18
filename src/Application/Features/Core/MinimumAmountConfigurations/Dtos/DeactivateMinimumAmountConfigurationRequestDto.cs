namespace TegWallet.Application.Features.Core.MinimumAmountConfigurations.Dtos;

public record DeactivateMinimumAmountConfigurationRequestDto(
    string Reason,
    string DeactivatedBy = "SYSTEM");