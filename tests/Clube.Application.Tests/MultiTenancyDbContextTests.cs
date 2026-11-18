using Clube.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clube.Application.Tests;

public class MultiTenancyDbContextTests
{
    [Fact]
    public async Task GlobalQueryFilter_DeveIsolarDadosPorClubeQuandoNaoEhSuperAdmin()
    {
        await using var scope = await TestDbScope.CreateSeededAsync(DbInitializer.ClubeFozId, isSuperAdmin: false);

        var associados = await scope.Db.Associados.ToListAsync();
        var espacos = await scope.Db.Espacos.ToListAsync();

        Assert.NotEmpty(associados);
        Assert.All(associados, a => Assert.Equal(DbInitializer.ClubeFozId, a.ClubeId));
        Assert.DoesNotContain(associados, a => a.ClubeId == DbInitializer.ClubeGuairaId);
        Assert.All(espacos, e => Assert.Equal(DbInitializer.ClubeFozId, e.ClubeId));
    }

    [Fact]
    public async Task GlobalQueryFilter_DeveRetornarTodosOsClubesQuandoSuperAdmin()
    {
        await using var scope = await TestDbScope.CreateSeededAsync(clubeId: null, isSuperAdmin: true);

        var associados = await scope.Db.Associados.ToListAsync();

        Assert.Contains(associados, a => a.ClubeId == DbInitializer.ClubeFozId);
        Assert.Contains(associados, a => a.ClubeId == DbInitializer.ClubeGuairaId);
    }
}
