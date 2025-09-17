using Agrovet.Migrator.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Agrovet.Infrastructure.Persistence.Context;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var optionsBuilder = new DbContextOptionsBuilder<AgrovetContext>();
optionsBuilder
    .UseNpgsql(config.GetConnectionString("DestConn"))
    .UseSnakeCaseNamingConvention();

await using var context = new AgrovetContext(optionsBuilder.Options);

var operationMigrator = new OperationMigrator(config, context);
await operationMigrator.MigrateAsync();

Console.WriteLine("Migration complete!");