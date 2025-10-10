using Microsoft.EntityFrameworkCore;
using Transfer.Application.Helpers;
using Transfer.Application.Interfaces.Sales;
using Transfer.Domain.Entity.Sales;
using Transfer.Infrastructure.Persistence.Context;

namespace Transfer.Infrastructure.Persistence.Repository.Sales;

public class PriceItemRepository(AgrovetContext context) :Disposable, IPriceItemRepository
{
    public async Task<PriceItem[]> GetAllAsync() => await context.PriceItemSet.ToArrayAsync();
}
