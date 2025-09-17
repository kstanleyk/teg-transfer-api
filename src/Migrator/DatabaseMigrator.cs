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
        var operationMigrator = new OperationMigrator(_connectionString!, context);
        await operationMigrator.MigrateAsync();
    }
}