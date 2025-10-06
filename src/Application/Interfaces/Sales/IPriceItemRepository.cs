using Agrovet.Domain.Entity.Sales;

namespace Agrovet.Application.Interfaces.Sales;

public interface IPriceItemRepository 
{
    Task<PriceItem[]> GetAllAsync();
}