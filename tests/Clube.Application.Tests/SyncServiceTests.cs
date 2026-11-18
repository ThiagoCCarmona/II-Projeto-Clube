using System.Text.Json;
using Clube.Domain.Enums;
using Clube.Infrastructure.Persistence;
using Clube.Infrastructure.Services;
using Clube.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Clube.Application.Tests;

public class SyncServiceTests
{
    [Fact]
    public async Task PushEPull_DeveSincronizarOutboxOfflineERetornarSnapshotDoTenant()
    {
        await using var scope = await TestDbScope.CreateSeededAsync(DbInitializer.ClubeFozId, isSuperAdmin: false);

        var reservaService = new ReservaService(scope.Db, scope.TenantContext);
        var mensalidadeService = new MensalidadeService(scope.Db, scope.TenantContext);
        var solicitacaoService = new SolicitacaoService(scope.Db, scope.TenantContext);
        var eventoService = new EventoService(scope.Db, scope.TenantContext);
        var syncService = new SyncService(
            scope.Db,
            scope.TenantContext,
            reservaService,
            mensalidadeService,
            solicitacaoService,
            eventoService);

        var espaco = await scope.Db.Espacos.FirstAsync();
        var associado = await scope.Db.Associados.FirstAsync(a => a.StatusAprovacao == StatusAprovacao.Aprovado);

        var reservaOfflineId = Guid.NewGuid();
        var payloadReserva = JsonSerializer.Serialize(new CriarReservaRequest(
            reservaOfflineId,
            espaco.Id,
            DbInitializer.ClubeFozId,
            associado.Id,
            FinalidadeUsoEspaco.Churrasco,
            new DateTime(2028, 3, 10, 9, 0, 0),
            new DateTime(2028, 3, 10, 18, 0, 0),
            "Criada sem internet no app",
            CriadoOffline: true));

        var pushResult = await syncService.PushAsync(new SyncPushRequest(
            DbInitializer.ClubeFozId,
            associado.Id,
            [new SyncMutationDto(Guid.NewGuid(), "RESERVA", payloadReserva, DateTime.UtcNow)]));

        Assert.Single(pushResult.Resultados);
        Assert.True(pushResult.Resultados[0].Sucesso);

        var pullResult = await syncService.PullAsync(DbInitializer.ClubeFozId, associado.Id);

        Assert.NotNull(pullResult.Clube);
        Assert.Contains(pullResult.Reservas, r => r.Id == reservaOfflineId && r.CriadoOffline && r.SincronizadoEm != null);
        Assert.NotEmpty(pullResult.Espacos);
        Assert.NotEmpty(pullResult.Eventos);
    }
}
