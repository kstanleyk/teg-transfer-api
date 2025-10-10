using Transfer.Application;
using Transfer.Infrastructure;
using Transfer.WebApi;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;
var host = builder.WebHost;

const string allowedCorsOrigins = nameof(allowedCorsOrigins);

services.AddApiServices(configuration, host, allowedCorsOrigins);
services.AddApplicationDependencies();
services.AddInfrastructureDependencies(configuration);
services.AddJwtAuthentication(configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.SeedDatabase();

app.UseCors(allowedCorsOrigins);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();