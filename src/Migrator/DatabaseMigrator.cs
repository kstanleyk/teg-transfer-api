using Agrovet.Infrastructure.Persistence.Context;
using Agrovet.Migrator.Core;
using Microsoft.Extensions.Configuration;
using Task = System.Threading.Tasks.Task;

namespace Agrovet.Migrator;

public class DatabaseMigrator(IConfiguration config, AgrovetContext context)
{
    private readonly string? _connectionString = config.GetConnectionString("SourceConn");
    
    public async Task MigrateAsync()
    {
        var averageWeightMigrator = new AverageWeightMigrator(_connectionString!, context);
        await averageWeightMigrator.MigrateAsync();

        var blockMigrator = new BlockMigrator(_connectionString!, context);
        await blockMigrator.MigrateAsync();

        var estateMigrator = new EstateMigrator(_connectionString!, context);
        await estateMigrator.MigrateAsync();

        var estateTaskMigrator = new EstateTaskMigrator(_connectionString!, context);
        await estateTaskMigrator.MigrateAsync();

        var operationMigrator = new OperationMigrator(_connectionString!, context);
        await operationMigrator.MigrateAsync();
    }
}