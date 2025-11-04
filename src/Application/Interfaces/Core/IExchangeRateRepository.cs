using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Interfaces.Core;

public interface IExchangeRateRepository : IRepository<ExchangeRate, Guid>
{

}