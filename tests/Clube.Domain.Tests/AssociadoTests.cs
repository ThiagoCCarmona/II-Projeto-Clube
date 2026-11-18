using Clube.Domain.Entities;
using Clube.Domain.Enums;

namespace Clube.Domain.Tests;

public class AssociadoTests
{
    private static Associado CriarAssociado(TipoAssociado tipo, StatusAprovacao status, Guid? titularId = null)
    {
        return new Associado
        {
            Id = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ClubeId = Guid.NewGuid(),
            Nome = "Associado Teste",
            CPF = "529.982.247-25",
            Email = "teste@clube.local",
            Telefone = "(45) 99999-0000",
            TipoAssociado = tipo,
            StatusAprovacao = status,
            SocioTitularId = titularId
        };
    }

    [Theory]
    [InlineData(TipoAssociado.SocioFundadorComVoto, StatusAprovacao.Aprovado, true)]
    [InlineData(TipoAssociado.SocioFundadorComVoto, StatusAprovacao.Pendente, false)]
    [InlineData(TipoAssociado.SocioFundadorComVoto, StatusAprovacao.Rejeitado, false)]
    [InlineData(TipoAssociado.SocioConvidadoSemVoto, StatusAprovacao.Aprovado, false)]
    [InlineData(TipoAssociado.Simpatizante, StatusAprovacao.Aprovado, false)]
    public void TemDireitoAVoto_DeveRespeitarTipoEStatus(
        TipoAssociado tipo,
        StatusAprovacao status,
        bool esperado)
    {
        var associado = CriarAssociado(tipo, status);
        Assert.Equal(esperado, associado.TemDireitoAVoto());
    }

    [Theory]
    [InlineData(TipoAssociado.SocioFundadorComVoto, StatusAprovacao.Aprovado, false, true)]
    [InlineData(TipoAssociado.SocioConvidadoSemVoto, StatusAprovacao.Aprovado, false, true)]
    [InlineData(TipoAssociado.SocioFundadorComVoto, StatusAprovacao.Pendente, false, false)]
    [InlineData(TipoAssociado.Simpatizante, StatusAprovacao.Aprovado, false, false)]
    [InlineData(TipoAssociado.SocioFundadorComVoto, StatusAprovacao.Aprovado, true, false)]
    public void DevePagarMensalidade_DeveCobrarApenasTitularesAprovados(
        TipoAssociado tipo,
        StatusAprovacao status,
        bool ehDependente,
        bool esperado)
    {
        var associado = CriarAssociado(tipo, status, ehDependente ? Guid.NewGuid() : null);
        Assert.Equal(esperado, associado.DevePagarMensalidade());
    }

    [Theory]
    [InlineData(TipoAssociado.SocioFundadorComVoto, StatusAprovacao.Aprovado, PublicoAlvoEvento.SomenteAssociados, true)]
    [InlineData(TipoAssociado.SocioConvidadoSemVoto, StatusAprovacao.Aprovado, PublicoAlvoEvento.SomenteAssociados, true)]
    [InlineData(TipoAssociado.Simpatizante, StatusAprovacao.Aprovado, PublicoAlvoEvento.SomenteAssociados, false)]
    [InlineData(TipoAssociado.SocioFundadorComVoto, StatusAprovacao.Pendente, PublicoAlvoEvento.SomenteAssociados, false)]
    [InlineData(TipoAssociado.Simpatizante, StatusAprovacao.Aprovado, PublicoAlvoEvento.Simpatizantes, true)]
    [InlineData(TipoAssociado.Simpatizante, StatusAprovacao.Pendente, PublicoAlvoEvento.Simpatizantes, false)]
    [InlineData(TipoAssociado.Simpatizante, StatusAprovacao.Pendente, PublicoAlvoEvento.AbertoAoPublico, true)]
    public void PodeParticiparDoEvento_DeveRespeitarPublicoAlvoEStatus(
        TipoAssociado tipo,
        StatusAprovacao status,
        PublicoAlvoEvento publico,
        bool esperado)
    {
        var associado = CriarAssociado(tipo, status);
        Assert.Equal(esperado, associado.PodeParticiparDoEvento(publico));
    }

    [Fact]
    public void Aprovar_DeveDefinirStatusAprovadoELimparMotivo()
    {
        var associado = CriarAssociado(TipoAssociado.SocioFundadorComVoto, StatusAprovacao.Rejeitado);
        associado.MotivoRejeicao = "Documentacao pendente";

        associado.Aprovar();

        Assert.Equal(StatusAprovacao.Aprovado, associado.StatusAprovacao);
        Assert.Null(associado.MotivoRejeicao);
    }

    [Fact]
    public void Rejeitar_DeveExigirMotivoValidoEDefinirStatus()
    {
        var associado = CriarAssociado(TipoAssociado.SocioFundadorComVoto, StatusAprovacao.Pendente);

        Assert.Throws<ArgumentException>(() => associado.Rejeitar("   "));

        associado.Rejeitar("  Cadastro incompleto  ");

        Assert.Equal(StatusAprovacao.Rejeitado, associado.StatusAprovacao);
        Assert.Equal("Cadastro incompleto", associado.MotivoRejeicao);
    }
}
