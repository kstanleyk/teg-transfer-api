using Agrovet.Domain.Entity.Core;
using Agrovet.Infrastructure.Persistence.Context;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SequentialGuid;
using Task = System.Threading.Tasks.Task;

namespace Agrovet.Migrator.Core;

public class AverageWeightMigrator(string connectionString, AgrovetContext context)
{
    public async Task MigrateAsync()
    {
        IEnumerable<AverageWeight> legacyItems;
        await using (var conn = new NpgsqlConnection(connectionString))
        {
            const string selectSql = """
                                 WITH averageWeight("id", estate, block, weight, effectiveDate, status, createdOn)
                                 AS
                                 (
                                 	SELECT  '24'||right(code,2) code, estate_code, block_code, weight, effective_date, status, created_on FROM core.average_weight
                                 )
                                 SELECT * FROM averageWeight;
                                 """;
            legacyItems = await conn.QueryAsync<AverageWeight>(selectSql);
        }

        var entities = new List<AverageWeight>();

        foreach (var item in legacyItems)
        {
            var entity = AverageWeight.Create(item.Id, item.Estate, item.Block, item.Weight, item.EffectiveDate,
                item.Status, item.CreatedOn);

            entity.SetPublicId(SequentialGuidGenerator.Instance.NewGuid());
            entities.Add(entity);
        }

        // Delete existing data from the destination table
        await context.AverageWeightSet.ExecuteDeleteAsync();
        await context.SaveChangesAsync();

        // Add new data
        await context.AverageWeightSet.AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }
}