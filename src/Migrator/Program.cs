using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Agrovet.Infrastructure.Persistence.Context;
using Agrovet.Migrator;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var optionsBuilder = new DbContextOptionsBuilder<AgrovetContext>();
optionsBuilder
    .UseNpgsql(config.GetConnectionString("DestConn"))
    .UseSnakeCaseNamingConvention();

await using var context = new AgrovetContext(optionsBuilder.Options);

var migrator = new DatabaseMigrator(config, context);
await migrator.MigrateAsync();

Console.WriteLine("Migration complete!");
Console.ReadKey();