using Agrovet.Domain.Entity.Core;
using Agrovet.Infrastructure.Persistence.Context;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Task = System.Threading.Tasks.Task;

namespace Agrovet.Migrator.Core;

public class OperationMigrator(IConfiguration config, AgrovetContext context)
{
    private readonly string? _connectionString = config.GetConnectionString("SourceConn");
    
    public async Task MigrateAsync()
    {
        IEnumerable<Operation> legacyOperations;
        await using (var conn = new NpgsqlConnection(_connectionString))
        {
            const string query = """
                                 WITH operations("id", line, payroll, employee, estate, block, item, millingCycle, description, quantity, rate, amount, transDate, status, syncReference, createdOn)
                                 AS
                                 (
                                 SELECT reference, ref_line, payroll, employee, estate, block, item, milling_cycle, description, quantity, rate, amount, trans_date, status, sync_reference, created_on
                                 	FROM core.activity
                                 UNION ALL
                                 SELECT reference, ref_line, payroll, employee, estate, block, item, milling_cycle, description, quantity, rate, amount, trans_date, status, sync_reference, created_on
                                 	FROM core.activity_history
                                 )
                                 SELECT * FROM operations
                                 """;
            legacyOperations = await conn.QueryAsync<Operation>(query);
        }

        var operations = new List<Operation>();

        foreach (var dto in legacyOperations)
        {
            var operation = Operation.Create(dto.Line, dto.Payroll, dto.Employee, dto.Estate, dto.Block, dto.Item,
                dto.MillingCycle, dto.Description, dto.Quantity, dto.Rate, dto.TransDate, dto.Status, dto.SyncReference,
                dto.CreatedOn);

            operation.SetId(dto.Id);
            operations.Add(operation);
        }

        // Delete existing data from the destination table
        await context.OperationSet.ExecuteDeleteAsync();
        await context.SaveChangesAsync();

        // Add new data
        await context.OperationSet.AddRangeAsync(operations);
        await context.SaveChangesAsync();
    }
}