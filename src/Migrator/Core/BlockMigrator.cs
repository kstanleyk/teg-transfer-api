using Agrovet.Domain.Entity.Core;
using Agrovet.Infrastructure.Persistence.Context;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SequentialGuid;
using Task = System.Threading.Tasks.Task;

namespace Agrovet.Migrator.Core;

public class BlockMigrator(string connectionString, AgrovetContext context)
{
    public async Task MigrateAsync()
    {
        IEnumerable<Block> legacyItems;
        await using (var conn = new NpgsqlConnection(connectionString))
        {
            const string selectSql = """
                                 WITH blocks(id, estate, description, treeNumber, dateEstablished, blockSize, createdOn)
                                 AS
                                 (
                                 SELECT code, estate, description, tree_number, date_established, block_size, created_on
                                 	FROM core.block
                                 )
                                 SELECT * FROM blocks;
                                 """;
            legacyItems = await conn.QueryAsync<Block>(selectSql);
        }

        var entities = new List<Block>();

        foreach (var item in legacyItems)
        {
            var entity = Block.Create(item.Estate, item.Description, item.TreeNumber, item.DateEstablished,
                item.BlockSize, item.CreatedOn);

            entity.SetId(item.Id);
            entity.SetPublicId(SequentialGuidGenerator.Instance.NewGuid());
            entities.Add(entity);
        }

        // Delete existing data from the destination table
        await context.BlockSet.ExecuteDeleteAsync();
        await context.SaveChangesAsync();

        // Add new data
        await context.BlockSet.AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }
}