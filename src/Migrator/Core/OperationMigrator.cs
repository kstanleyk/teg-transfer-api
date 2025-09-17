using Agrovet.Domain.Entity.Core;
using Agrovet.Infrastructure.Persistence.Context;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Task = System.Threading.Tasks.Task;

namespace Agrovet.Migrator.Core;

public class OperationMigrator(string connectionString, AgrovetContext context)
{
    public async Task MigrateAsync()
    {
        IEnumerable<Operation> legacyItems;
        await using (var conn = new NpgsqlConnection(connectionString))
        {
            const string selectSql = """
                                 WITH operations("id", line, payroll, employee, estate, block, item, millingCycle, description, quantity, rate, amount, transDate, status, syncReference, createdOn)
                                 AS
                                 (
                                 SELECT '24'||right(reference,5), ref_line, payroll, employee, estate, block, item, milling_cycle, description, quantity, rate, amount, trans_date, status, sync_reference, created_on
                                 	FROM core.activity
                                 UNION ALL
                                 SELECT '24'||right(reference,5), ref_line, payroll, employee, estate, block, item, milling_cycle, description, quantity, rate, amount, trans_date, '02' as status, sync_reference, created_on
                                 	FROM core.activity_history
                                 )
                                 SELECT * FROM operations
                                 """;
            legacyItems = await conn.QueryAsync<Operation>(selectSql);
        }

        var entities = new List<Operation>();

        foreach (var item in legacyItems)
        {
            var entity = Operation.Create(item.Line, item.Payroll, item.Employee, item.Estate, item.Block, item.Item,
                item.MillingCycle, item.Description, item.Quantity, item.Rate, item.TransDate, item.Status, item.SyncReference,
                item.CreatedOn);

            entity.SetId(item.Id);
            entities.Add(entity);
        }

        // Delete existing data from the destination table
        await context.OperationSet.ExecuteDeleteAsync();
        await context.SaveChangesAsync();

        // Add new data
        await context.OperationSet.AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }
}