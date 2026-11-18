using Clube.Domain.Enums;
using Clube.Infrastructure.Persistence;
using Clube.Infrastructure.Services;
using Clube.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Clube.Application.Tests;

public class ReservaServiceTests
{
    [Fact]
    public async Task SolicitarReserva_DevePermitirReservaContinuaMultiDiaSemConflito()
    {
        await using var scope = await TestDbScope.CreateSeededAsync(DbInitializer.ClubeFozId, isSuperAdmin: false);
        var service = new ReservaService(scope.Db, scope.TenantContext);

        var espaco = await scope.Db.Espacos.FirstAsync();
        var associado = await scope.Db.Associados.FirstAsync(a => a.StatusAprovacao == StatusAprovacao.Aprovado);

        var inicio = new DateTime(2028, 10, 31, 8, 0, 0);
        var fim = new DateTime(2028, 11, 1, 18, 0, 0);

        var reserva = await service.SolicitarReservaAsync(new CriarReservaRequest(
            null,
            espaco.Id,
            DbInitializer.ClubeFozId,
            associado.Id,
            FinalidadeUsoEspaco.Feijoada,
            inicio,
            fim,
            "Feijoada Beneficente de 2 dias"));

        Assert.Equal(StatusReserva.PendenteAprovacao, reserva.Status);
        Assert.Equal(inicio, reserva.DataHoraInicio);
        Assert.Equal(fim, reserva.DataHoraFim);
    }

    [Fact]
    public async Task SolicitarReserva_DeveMarcarConflito_QuandoSobrepoeReservaConfirmada()
    {
        await using var scope = await TestDbScope.CreateSeededAsync(DbInitializer.ClubeFozId, isSuperAdmin: false);
        var service = new ReservaService(scope.Db, scope.TenantContext);

        var espaco = await scope.Db.Espacos.FirstAsync();
        var associado = await scope.Db.Associados.FirstAsync(a => a.StatusAprovacao == StatusAprovacao.Aprovado);

        var r1 = await service.SolicitarReservaAsync(new CriarReservaRequest(
            null,
            espaco.Id,
            DbInitializer.ClubeFozId,
            associado.Id,
            FinalidadeUsoEspaco.Festa,
            new DateTime(2027, 12, 5, 10, 0, 0),
            new DateTime(2027, 12, 5, 18, 0, 0),
            "Reserva confirmada"));

        await service.AprovarReservaAsync(r1.Id);

        var r2 = await service.SolicitarReservaAsync(new CriarReservaRequest(
            null,
            espaco.Id,
            DbInitializer.ClubeFozId,
            associado.Id,
            FinalidadeUsoEspaco.Churrasco,
            new DateTime(2027, 12, 5, 14, 0, 0),
            new DateTime(2027, 12, 5, 22, 0, 0),
            "Reserva concorrente"));

        Assert.Equal(StatusReserva.Conflito, r2.Status);
    }

    [Fact]
    public async Task AprovarReserva_DeveRejeitarConcorrentesPendentesPorConflito()
    {
        await using var scope = await TestDbScope.CreateSeededAsync(DbInitializer.ClubeFozId, isSuperAdmin: false);
        var service = new ReservaService(scope.Db, scope.TenantContext);

        var espaco = await scope.Db.Espacos.FirstAsync();
        var associado = await scope.Db.Associados.FirstAsync(a => a.StatusAprovacao == StatusAprovacao.Aprovado);

        var inicio = new DateTime(2027, 11, 20, 9, 0, 0);
        var fim = new DateTime(2027, 11, 20, 17, 0, 0);

        var pendente1 = await service.SolicitarReservaAsync(new CriarReservaRequest(
            null, espaco.Id, DbInitializer.ClubeFozId, associado.Id,
            FinalidadeUsoEspaco.Festa, inicio, fim, "Solicitacao A"));

        var pendente2 = await service.SolicitarReservaAsync(new CriarReservaRequest(
            null, espaco.Id, DbInitializer.ClubeFozId, associado.Id,
            FinalidadeUsoEspaco.Churrasco, inicio.AddHours(1), fim.AddHours(1), "Solicitacao B"));

        Assert.True(pendente2.TemConflitoPendente);

        var aprovada = await service.AprovarReservaAsync(pendente1.Id);
        var lista = await service.ListarReservasAsync(DbInitializer.ClubeFozId);
        var concorrenteAtualizada = lista.First(r => r.Id == pendente2.Id);

        Assert.NotNull(aprovada);
        Assert.Equal(StatusReserva.Confirmada, aprovada.Status);
        Assert.Equal(StatusReserva.RejeitadaPorConflito, concorrenteAtualizada.Status);
    }
}
