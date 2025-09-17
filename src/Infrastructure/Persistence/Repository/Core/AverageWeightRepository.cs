using System.Globalization;
using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Core;
using Agrovet.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace Agrovet.Infrastructure.Persistence.Repository.Core;

public class AverageWeightRepository(IDatabaseFactory databaseFactory)
    : Repository<AverageWeight, string>(databaseFactory), IAverageWeightRepository
{
    public override async Task<RepositoryActionResult<AverageWeight>> AddAsync(AverageWeight averageWeight)
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
            averageWeight.SetId(newId);

            await DbSet.AddAsync(averageWeight);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<AverageWeight>(averageWeight, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<AverageWeight>(null, RepositoryActionStatus.Error, ex);
        }
    }
}