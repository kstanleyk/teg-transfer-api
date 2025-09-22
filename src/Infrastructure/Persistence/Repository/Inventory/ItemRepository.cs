using System.Globalization;
using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.Entity.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Agrovet.Infrastructure.Persistence.Repository.Inventory;

public class ItemRepository(IDatabaseFactory databaseFactory)
    : Repository<Item, string>(databaseFactory), IItemRepository
{
    public override async Task<RepositoryActionResult<Item>> AddAsync(Item item)
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
            item.SetId(newId);

            await DbSet.AddAsync(item);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<Item>(item, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Item>(null, RepositoryActionStatus.Error, ex);
        }
    }
}