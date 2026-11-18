using System.Text.Json;
using Clube.Application.Interfaces;
using Clube.Infrastructure.Persistence;
using Clube.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Clube.Infrastructure.Services;

public class SyncService : ISyncService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly ClubeDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IReservaService _reservaService;
    private readonly IMensalidadeService _mensalidadeService;
    private readonly ISolicitacaoService _solicitacaoService;
    private readonly IEventoService _eventoService;

    public SyncService(
        ClubeDbContext db,
        ITenantContext tenantContext,
        IReservaService reservaService,
        IMensalidadeService mensalidadeService,
        ISolicitacaoService solicitacaoService,
        IEventoService eventoService)
    {
        _db = db;
        _tenantContext = tenantContext;
        _reservaService = reservaService;
        _mensalidadeService = mensalidadeService;
        _solicitacaoService = solicitacaoService;
        _eventoService = eventoService;
    }

    public async Task<SyncPushResponse> PushAsync(SyncPushRequest request, CancellationToken ct = default)
    {
        var resultados = new List<SyncItemResultDto>();

        foreach (var mutacao in request.Mutacoes.OrderBy(m => m.CriadoEmUtc))
        {
            try
            {
                switch (mutacao.TipoOperacao.Trim().ToUpperInvariant())
                {
                    case "RESERVA":
                        var reqReserva = JsonSerializer.Deserialize<CriarReservaRequest>(mutacao.PayloadJson, JsonOpts)
                            ?? throw new InvalidOperationException("Payload de reserva inválido.");
                        var resReserva = await _reservaService.SolicitarReservaAsync(
                            reqReserva with { CriadoOffline = true }, ct);
                        resultados.Add(new SyncItemResultDto(
                            mutacao.OutboxId,
                            true,
                            resReserva.Status.ToString(),
                            resReserva.Status == Domain.Enums.StatusReserva.Conflito
                                ? "Sincronizada com conflito de horário."
                                : "Reserva sincronizada com sucesso.",
                            resReserva.Id));
                        break;

                    case "SOLICITACAO":
                        var reqSac = JsonSerializer.Deserialize<CriarSolicitacaoRequest>(mutacao.PayloadJson, JsonOpts)
                            ?? throw new InvalidOperationException("Payload de solicitação inválido.");
                        var resSac = await _solicitacaoService.CriarAsync(reqSac, ct);
                        resultados.Add(new SyncItemResultDto(
                            mutacao.OutboxId,
                            true,
                            resSac.Status.ToString(),
                            "Solicitação sincronizada com sucesso.",
                            resSac.Id));
                        break;

                    case "INSCRICAO_EVENTO":
                        var payload = JsonSerializer.Deserialize<InscricaoOutboxPayload>(mutacao.PayloadJson, JsonOpts)
                            ?? throw new InvalidOperationException("Payload de inscrição inválido.");
                        var resInsc = await _eventoService.InscreverAsync(
                            payload.EventoId,
                            new InscreverEventoRequest(payload.AssociadoId, payload.QuantidadeAcompanhantes),
                            ct);
                        resultados.Add(new SyncItemResultDto(
                            mutacao.OutboxId,
                            true,
                            resInsc.Status.ToString(),
                            "Inscrição no evento confirmada.",
                            resInsc.Id));
                        break;

                    default:
                        resultados.Add(new SyncItemResultDto(
                            mutacao.OutboxId,
                            false,
                            "Ignorada",
                            $"Tipo de operação '{mutacao.TipoOperacao}' não suportado.",
                            null));
                        break;
                }
            }
            catch (Exception ex)
            {
                resultados.Add(new SyncItemResultDto(
                    mutacao.OutboxId,
                    false,
                    "Erro",
                    ex.Message,
                    null));
            }
        }

        return new SyncPushResponse(resultados, DateTime.UtcNow);
    }

    public async Task<SyncPullResponse> PullAsync(Guid clubeId, Guid? associadoId, CancellationToken ct = default)
    {
        var clubeAlvo = _tenantContext.CurrentClubeId ?? clubeId;

        var clube = await _db.Clubes.FirstOrDefaultAsync(c => c.Id == clubeAlvo, ct);
        AssociadoDto? associado = null;

        if (associadoId.HasValue)
        {
            var assocEntity = await _db.Associados
                .Include(a => a.Clube)
                .Include(a => a.Dependentes)
                .FirstOrDefaultAsync(a => a.Id == associadoId.Value, ct);
            associado = assocEntity?.ToDto();
        }

        var espacos = await _reservaService.ListarEspacosAsync(clubeAlvo, ct);
        var eventos = await _eventoService.ListarAsync(clubeAlvo, ct);
        var reservas = await _reservaService.ListarReservasAsync(clubeAlvo, ct);
        var mensalidades = await _mensalidadeService.ListarAsync(clubeAlvo, associadoId, ct);
        var solicitacoes = await _solicitacaoService.ListarAsync(clubeAlvo, associadoId, ct);

        return new SyncPullResponse(
            clube?.ToDto(),
            associado,
            espacos,
            eventos,
            reservas,
            mensalidades,
            solicitacoes,
            DateTime.UtcNow);
    }

    private sealed record InscricaoOutboxPayload(Guid EventoId, Guid AssociadoId, int QuantidadeAcompanhantes);
}
