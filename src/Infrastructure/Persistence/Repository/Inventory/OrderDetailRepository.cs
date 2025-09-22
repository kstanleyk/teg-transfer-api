using System.Globalization;
using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.Entity.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Agrovet.Infrastructure.Persistence.Repository.Inventory;

public class OrderDetailRepository(IDatabaseFactory databaseFactory)
    : Repository<OrderDetail, string>(databaseFactory), IOrderDetailRepository
{
    public override async Task<RepositoryActionResult<OrderDetail>> AddAsync(OrderDetail orderDetail)
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
            orderDetail.SetId(newId);

            await DbSet.AddAsync(orderDetail);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<OrderDetail>(orderDetail, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<OrderDetail>(null, RepositoryActionStatus.Error, ex);
        }
    }
}