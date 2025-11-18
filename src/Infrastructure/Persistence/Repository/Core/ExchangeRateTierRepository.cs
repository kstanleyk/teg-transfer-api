using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class ExchangeRateTierRepository(IDatabaseFactory databaseFactory)
    : DataRepository<ExchangeRateTier, Guid>(databaseFactory), IExchangeRateTierRepository
{

}