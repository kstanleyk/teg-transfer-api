using Agrovet.Domain.Entity.Core;
using Agrovet.Infrastructure.Persistence.Context;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SequentialGuid;
using Task = System.Threading.Tasks.Task;

namespace Agrovet.Migrator.Core;

public class EstateTaskMigrator(string connectionString, AgrovetContext context)
{
    public async Task MigrateAsync()
    {
        IEnumerable<EstateTask> legacyItems;
        await using (var conn = new NpgsqlConnection(connectionString))
        {
            const string selectSql = """
                                 WITH estateTasks(id, taskId, estateId, rate, effectiveDate, createdOn)
                                 AS
                                 (
                                 SELECT code, activity_code, estate_code, rate, effective_date, created_on
                                 	FROM core.estate_activity
                                 )
                                 SELECT * FROM estateTasks;
                                 """;
            legacyItems = await conn.QueryAsync<EstateTask>(selectSql);
        }

        var entities = new List<EstateTask>();

        foreach (var item in legacyItems)
        {
            var entity = EstateTask.Create(item.Id, item.TaskId, item.EstateId, item.Rate, item.EffectiveDate,
                item.CreatedOn);

            entity.SetId(item.Id);
            entity.SetPublicId(SequentialGuidGenerator.Instance.NewGuid());
            entities.Add(entity);
        }

        // Delete existing data from the destination table
        await context.EstateTaskSet.ExecuteDeleteAsync();
        await context.SaveChangesAsync();

        // Add new data
        await context.EstateTaskSet.AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }
}