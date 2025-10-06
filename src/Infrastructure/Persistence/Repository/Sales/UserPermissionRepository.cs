using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Sales;
using Agrovet.Domain.Entity.Sales;
using Agrovet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Agrovet.Infrastructure.Persistence.Repository.Sales;

public class PriceItemRepository(AgrovetContext context) :Disposable, IPriceItemRepository
{
    public async Task<PriceItem[]> GetAllAsync() => await context.PriceItemSet.ToArrayAsync();
}
