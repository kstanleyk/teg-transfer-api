using Agrovet.Domain.Entity.Core;
using Agrovet.Infrastructure.Persistence.Context;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SequentialGuid;
using Task = System.Threading.Tasks.Task;

namespace Agrovet.Migrator.Core;

public class EstateMigrator(string connectionString, AgrovetContext context)
{
    public async Task MigrateAsync()
    {
        IEnumerable<Estate> legacyItems;
        await using (var conn = new NpgsqlConnection(connectionString))
        {
            const string selectSql = """
                                 WITH estates(id, description, location, dateEstablished, createdOn)
                                 AS
                                 (
                                 SELECT code, description, location, date_established, created_on
                                 	FROM core.estate order by 1
                                 )
                                 SELECT * FROM estates;
                                 """;
            legacyItems = await conn.QueryAsync<Estate>(selectSql);
        }

        var entities = new List<Estate>();

        foreach (var item in legacyItems)
        {
            var entity = Estate.Create(item.Description, item.Location, item.DateEstablished, item.CreatedOn);

            entity.SetId(item.Id);
            entity.SetPublicId(SequentialGuidGenerator.Instance.NewGuid());
            entities.Add(entity);
        }

        // Delete existing data from the destination table
        await context.EstateSet.ExecuteDeleteAsync();
        await context.SaveChangesAsync();

        // Add new data
        await context.EstateSet.AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }
}