using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class ExchangeRateHistoryRepository(IDatabaseFactory databaseFactory)
    : DataRepository<ExchangeRateHistory, Guid>(databaseFactory), IExchangeRateHistoryRepository
{

}