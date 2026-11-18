using Clube.Application.Payments;
using Clube.Domain.Enums;
using Clube.Infrastructure.Persistence;
using Clube.Infrastructure.Services;
using Clube.Shared.DTOs;

namespace Clube.Application.Tests;

public class MensalidadeServiceTests
{
    [Fact]
    public async Task GerarLote_DeveCobrarApenasTitularesAprovadosEIsentarSimpatizantesEPendentes()
    {
        await using var scope = await TestDbScope.CreateSeededAsync(DbInitializer.ClubeFozId, isSuperAdmin: false);
        var service = new MensalidadeService(scope.Db, scope.TenantContext);

        var resultado = await service.GerarLoteAsync(new GerarLoteMensalidadeRequest(
            DbInitializer.ClubeFozId,
            "12/2027",
            new DateTime(2027, 12, 10)));

        // No seed de Foz existem: Presidente(Fundador Aprovado), Tesoureiro(Fundador Aprovado),
        // SocioFundador(Aprovado), SocioConvidado(Aprovado) -> 4 titulares aprovados.
        // Simpatizante e Candidato Pendente devem ser ignorados.
        Assert.True(resultado.Geradas >= 2);
        Assert.All(resultado.MensalidadesGeradas, m =>
        {
            Assert.NotEqual(TipoAssociado.Simpatizante, m.TipoAssociado);
            Assert.True(PixPayloadGenerator.ValidarChecksum(m.CodigoPixCopiaECola));
            Assert.Equal(StatusMensalidade.Pendente, m.Status);
        });
        Assert.True(resultado.IsentosOuNaoTitulares >= 2);

        // Idempotência na segunda execução da mesma competência
        var segundaExecucao = await service.GerarLoteAsync(new GerarLoteMensalidadeRequest(
            DbInitializer.ClubeFozId,
            "12/2027",
            new DateTime(2027, 12, 10)));

        Assert.Equal(0, segundaExecucao.Geradas);
        Assert.Equal(resultado.Geradas, segundaExecucao.JaExistentes);
    }

    [Fact]
    public async Task Baixar_DeveMarcarMensalidadeComoPaga()
    {
        await using var scope = await TestDbScope.CreateSeededAsync(DbInitializer.ClubeFozId, isSuperAdmin: false);
        var service = new MensalidadeService(scope.Db, scope.TenantContext);

        var lote = await service.GerarLoteAsync(new GerarLoteMensalidadeRequest(
            DbInitializer.ClubeFozId,
            "01/2028",
            new DateTime(2028, 1, 10)));

        var primeira = lote.MensalidadesGeradas.First();
        var baixada = await service.BaixarAsync(primeira.Id, "Pago via PIX na secretaria");

        Assert.NotNull(baixada);
        Assert.Equal(StatusMensalidade.Paga, baixada.Status);
        Assert.NotNull(baixada.DataPagamento);
    }
}
