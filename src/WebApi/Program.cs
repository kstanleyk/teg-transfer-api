using Microsoft.Extensions.Options;
using TegWallet.Application;
using TegWallet.Infrastructure;
using TegWallet.WebApi;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;
var host = builder.WebHost;

const string allowedCorsOrigins = nameof(allowedCorsOrigins);

services.AddApiServices(configuration, host, allowedCorsOrigins);
services.AddApplicationDependencies();
services.AddInfrastructureDependencies(configuration);
services.AddJwtAuthentication(configuration);
services.AddVersioning();
services.AddLocalizationServices();

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
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
app.MapControllers();

app.Run();