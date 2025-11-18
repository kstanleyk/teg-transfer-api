using TegWallet.Application.Features.Core.MinimumAmountConfigurations.Command;
using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Interfaces.Core;

public interface IMinimumAmountConfigurationRepository : IRepository<MinimumAmountConfiguration, Guid>
{
    Task<RepositoryActionResult<MinimumAmountConfiguration>> CreateAsync(CreateMinimumAmountConfigurationParameters parameters);
    Task<RepositoryActionResult<MinimumAmountConfiguration>> UpdateAsync(UpdateMinimumAmountConfigurationParameters parameters);

    Task<MinimumAmountConfiguration?> GetApplicableMinimumAmountAsync(
        Currency? baseCurrency,
        Currency? targetCurrency,
        DateTime asOfDate);

    Task<IReadOnlyList<MinimumAmountConfiguration>> GetActiveConfigurationsAsync(
        Currency? baseCurrency = null,
        Currency? targetCurrency = null,
        DateTime? asOfDate = null);

    Task<IReadOnlyList<MinimumAmountConfiguration>> GetOverlappingConfigurationsAsync(
        Currency baseCurrency,
        Currency targetCurrency,
        DateTime effectiveFrom,
        DateTime? effectiveTo);

    Task<MinimumAmountConfiguration?> GetByIdAsync(Guid id);

    Task<RepositoryActionResult<MinimumAmountConfiguration>> DeactivateAsync(
        DeactivateMinimumAmountConfigurationParameters parameters);
}