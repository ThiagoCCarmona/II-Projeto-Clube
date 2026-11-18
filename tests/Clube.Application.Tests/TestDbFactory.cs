using Clube.Application.Interfaces;
using Clube.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Clube.Application.Tests;

internal sealed class TestDbScope : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    public ITenantContext TenantContext { get; }
    public ClubeDbContext Db { get; }

    private TestDbScope(SqliteConnection connection, ITenantContext tenantContext, ClubeDbContext db)
    {
        _connection = connection;
        TenantContext = tenantContext;
        Db = db;
    }

    public static async Task<TestDbScope> CreateSeededAsync(Guid? clubeId = null, bool isSuperAdmin = true)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var tenant = new TenantContext();
        tenant.SetTenant(clubeId, isSuperAdmin);

        var options = new DbContextOptionsBuilder<ClubeDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ClubeDbContext(options, tenant);
        await db.Database.EnsureCreatedAsync();
        await DbInitializer.SeedAsync(db);

        return new TestDbScope(connection, tenant, db);
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
