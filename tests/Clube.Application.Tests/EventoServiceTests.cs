using Clube.Domain.Enums;
using Clube.Infrastructure.Persistence;
using Clube.Infrastructure.Services;
using Clube.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Clube.Application.Tests;

public class EventoServiceTests
{
    [Fact]
    public async Task Inscrever_DeveBloquearSimpatizanteEmEventoSomenteAssociados()
    {
        await using var scope = await TestDbScope.CreateSeededAsync(DbInitializer.ClubeFozId, isSuperAdmin: false);
        var service = new EventoService(scope.Db, scope.TenantContext);

        var eventoRestrito = await service.CriarAsync(new CriarEventoRequest(
            DbInitializer.ClubeFozId,
            "Assembleia Exclusiva de Socios",
            "Reuniao restrita",
            TipoEvento.Geral,
            PublicoAlvoEvento.SomenteAssociados,
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(10).AddHours(3),
            "Salao Social",
            0m));

        var simpatizante = await scope.Db.Associados.FirstAsync(a => a.TipoAssociado == TipoAssociado.Simpatizante);
        var socioFundador = await scope.Db.Associados.FirstAsync(a =>
            a.TipoAssociado == TipoAssociado.SocioFundadorComVoto &&
            a.StatusAprovacao == StatusAprovacao.Aprovado);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InscreverAsync(eventoRestrito.Id, new InscreverEventoRequest(simpatizante.Id, 0)));

        var inscricaoValida = await service.InscreverAsync(
            eventoRestrito.Id,
            new InscreverEventoRequest(socioFundador.Id, 2));

        Assert.Equal(StatusInscricao.Confirmada, inscricaoValida.Status);
        Assert.Equal(2, inscricaoValida.QuantidadeAcompanhantes);
    }
}
