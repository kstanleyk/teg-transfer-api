using Transfer.Domain.Entity.Sales;

namespace Transfer.Application.Interfaces.Sales;

public interface IPriceItemRepository 
{
    Task<PriceItem[]> GetAllAsync();
}