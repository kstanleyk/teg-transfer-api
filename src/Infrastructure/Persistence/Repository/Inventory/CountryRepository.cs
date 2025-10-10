using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Transfer.Application.Helpers;
using Transfer.Application.Interfaces.Inventory;
using Transfer.Domain.Entity.Inventory;

namespace Transfer.Infrastructure.Persistence.Repository.Inventory;

public class CountryRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Country, string>(databaseFactory), ICountryRepository
{
    public override async Task<RepositoryActionResult<Country>> AddAsync(Country country)
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

            var newId = (lastNumber + 1).ToString(CultureInfo.InvariantCulture).PadLeft(2,'0');
            country.SetId(newId);

            await DbSet.AddAsync(country);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<Country>(country, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Country>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public override async Task<RepositoryActionResult<Country>> UpdateAsyncAsync(Guid publicId,
        Country country)
    {
        try
        {
            var existingEntity = await GetByPublicIdAsync(publicId);
            if (existingEntity == null)
                return new RepositoryActionResult<Country>(null, RepositoryActionStatus.NotFound);

            // Return early if no changes
            if (!existingEntity.HasChanges(country))
                return new RepositoryActionResult<Country>(existingEntity, RepositoryActionStatus.NothingModified);

            var entry = Context.Entry(existingEntity);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(existingEntity);

            existingEntity.Update(country);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await entry.ReloadAsync();
                return new RepositoryActionResult<Country>(existingEntity, RepositoryActionStatus.Updated);
            }

            return new RepositoryActionResult<Country>(existingEntity, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<Country>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            return new RepositoryActionResult<Country>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Country>(null, RepositoryActionStatus.Error, ex);
        }
    }
}