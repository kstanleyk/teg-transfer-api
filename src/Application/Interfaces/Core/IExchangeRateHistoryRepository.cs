using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Interfaces.Core;

public interface IExchangeRateHistoryRepository : IRepository<ExchangeRateHistory, Guid>
{
    Task<IReadOnlyList<ExchangeRateHistory>> GetExchangeRateChangeHistoryAsync(Guid exchangeRateId);
}