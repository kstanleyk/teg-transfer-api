using System.Globalization;
using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.Entity.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Agrovet.Infrastructure.Persistence.Repository.Inventory;

public class OrderRepository(IDatabaseFactory databaseFactory)
    : Repository<Order, string>(databaseFactory), IOrderRepository
{
    public override async Task<RepositoryActionResult<Order>> AddAsync(Order order)
    {
        try
        {
            var lastIdValue = await DbSet
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            var lastNumber = string.IsNullOrWhiteSpace(lastIdValue)
                ? 0
                : lastIdValue.ToNumValue();

            var newId = (lastNumber + 1).ToString(CultureInfo.InvariantCulture).PadLeft(3,'0');
            order.SetId(newId);

            await DbSet.AddAsync(order);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<Order>(order, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Order>(null, RepositoryActionStatus.Error, ex);
        }
    }
}