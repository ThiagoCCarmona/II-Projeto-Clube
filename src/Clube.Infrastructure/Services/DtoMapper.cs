using Clube.Domain.Entities;
using Clube.Shared.DTOs;

namespace Clube.Infrastructure.Services;

internal static class DtoMapper
{
    public static ClubeDto ToDto(this Domain.Entities.Clube c) => new(
        c.Id, c.Nome, c.Sigla, c.Endereco, c.Bairro, c.Cidade, c.UF,
        c.Telefone, c.Email, c.ChavePix, c.TipoChavePix,
        c.ValorMensalidadePadrao, c.Ativo, c.DataCadastro);

    public static DependenteDto ToDto(this Dependente d) => new(
        d.Id, d.ClubeId, d.AssociadoTitularId, d.Nome, d.CPF,
        d.GrauParentesco, d.DataNascimento, d.Email, d.Telefone, d.Ativo);

    public static AssociadoDto ToDto(this Associado a) => new(
        a.Id,
        a.UsuarioId,
        a.ClubeId,
        a.Clube?.Nome ?? string.Empty,
        a.Nome,
        a.CPF,
        a.Email,
        a.Telefone,
        a.TipoAssociado,
        a.StatusAprovacao,
        a.TemDireitoAVoto(),
        a.DataNascimento,
        a.Sexo,
        a.EstadoCivil,
        a.Endereco,
        a.Bairro,
        a.Cidade,
        a.UF,
        a.Interesses,
        a.SocioTitularId,
        a.MotivoRejeicao,
        a.DataCadastro,
        a.Dependentes.Select(d => d.ToDto()).ToList());

    public static EventoDto ToDto(this Evento e) => new(
        e.Id,
        e.ClubeId,
        e.Titulo,
        e.Descricao,
        e.TipoEvento,
        e.PublicoAlvo,
        e.DataInicio,
        e.DataFim,
        e.Local,
        e.ValorIngresso,
        e.RequerInscricao,
        e.Ativo,
        e.Inscricoes.Count(i => i.Status == Domain.Enums.StatusInscricao.Confirmada));

    public static EspacoDto ToDto(this Espaco e) => new(
        e.Id, e.ClubeId, e.Nome, e.Capacidade, e.Descricao, e.TaxaReserva, e.Ativo);

    public static ReservaEspacoDto ToDto(this ReservaEspaco r, bool temConflitoPendente = false) => new(
        r.Id,
        r.EspacoId,
        r.Espaco?.Nome ?? string.Empty,
        r.ClubeId,
        r.AssociadoId,
        r.Associado?.Nome ?? string.Empty,
        r.FinalidadeUso,
        r.DataHoraInicio,
        r.DataHoraFim,
        r.Status,
        r.Observacoes,
        r.CriadoOffline,
        r.SincronizadoEm,
        temConflitoPendente);

    public static MensalidadeDto ToDto(this Mensalidade m) => new(
        m.Id,
        m.ClubeId,
        m.AssociadoId,
        m.Associado?.Nome ?? string.Empty,
        m.Associado?.CPF ?? string.Empty,
        m.Associado?.TipoAssociado ?? Domain.Enums.TipoAssociado.SocioFundadorComVoto,
        m.Competencia,
        m.Valor,
        m.DataVencimento,
        m.DataPagamento,
        m.Status,
        m.CodigoPixCopiaECola,
        m.Observacoes);

    public static SolicitacaoDto ToDto(this SolicitacaoReclamacao s) => new(
        s.Id,
        s.ClubeId,
        s.AssociadoId,
        s.Associado?.Nome ?? string.Empty,
        s.Tipo,
        s.Assunto,
        s.Descricao,
        s.RespostaDiretoria,
        s.Status,
        s.DataCriacao,
        s.DataResposta);
}
