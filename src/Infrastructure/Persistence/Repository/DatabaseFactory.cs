using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Transfer.Application.Helpers;
using Transfer.Infrastructure.Persistence.Context;

namespace Transfer.Infrastructure.Persistence.Repository;

public class DatabaseFactory : Disposable, IDatabaseFactory
{
    public DatabaseFactory(TransferContext dataContext)
    {
        _dataContext = dataContext;
        _db = new NpgsqlConnection(GetContext().Database.GetDbConnection().ConnectionString);
    }

    private readonly TransferContext _dataContext;
    private readonly IDbConnection _db;

    public TransferContext GetContext()
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
    TransferContext GetContext();
    IDbConnection GetConnection();
}