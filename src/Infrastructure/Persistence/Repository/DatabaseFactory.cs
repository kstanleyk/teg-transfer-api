using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TegWallet.Application.Helpers;
using TegWallet.Infrastructure.Persistence.Context;

namespace TegWallet.Infrastructure.Persistence.Repository;

public class DatabaseFactory : Disposable, IDatabaseFactory
{
    public DatabaseFactory(TegWalletContext dataContext)
    {
        _dataContext = dataContext;
        _db = new NpgsqlConnection(GetContext().Database.GetDbConnection().ConnectionString);
    }

    private readonly TegWalletContext _dataContext;
    private readonly IDbConnection _db;

    public TegWalletContext GetContext()
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
    TegWalletContext GetContext();
    IDbConnection GetConnection();
}