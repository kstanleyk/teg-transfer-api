using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Transfer.Application.Helpers;
using Transfer.Infrastructure.Persistence.Context;

namespace Transfer.Infrastructure.Persistence.Repository;

public class DatabaseFactory : Disposable, IDatabaseFactory
{
    public DatabaseFactory(AgrovetContext dataContext)
    {
        _dataContext = dataContext;
        _db = new NpgsqlConnection(GetContext().Database.GetDbConnection().ConnectionString);
    }

    private readonly AgrovetContext _dataContext;
    private readonly IDbConnection _db;

    public AgrovetContext GetContext()
    {
        return _dataContext;
    }

    public IDbConnection GetConnection()
    {
        return _db;
    }

    protected override void DisposeCore()
    {
        _dataContext.Dispose();
    }
}

public interface IDatabaseFactory : IDisposable
{
    AgrovetContext GetContext();
    IDbConnection GetConnection();
}