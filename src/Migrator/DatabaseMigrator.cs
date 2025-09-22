using Agrovet.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Configuration;
using Task = System.Threading.Tasks.Task;

namespace Agrovet.Migrator;

public class DatabaseMigrator(IConfiguration config, AgrovetContext context)
{
    private readonly string? _connectionString = config.GetConnectionString("SourceConn");
    
    public async Task MigrateAsync()
    {
        //var averageWeightMigrator = new AverageWeightMigrator(_connectionString!, context);
        //await averageWeightMigrator.MigrateAsync();
    }
}