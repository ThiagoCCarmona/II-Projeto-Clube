using System.Text.Json;
using Clube.Domain.Enums;
using Clube.Mobile.Models;
using Clube.Shared.DTOs;

namespace Clube.Mobile.Services;

/// <summary>
/// Orquestrador Offline-First: grava na fila Outbox local quando offline e sincroniza Push/Pull quando online.
/// </summary>
public class MobileSyncService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly LocalDatabaseService _localDb;
    private readonly ApiClient _api;
    private readonly SessionStateService _session;

    public MobileSyncService(LocalDatabaseService localDb, ApiClient api, SessionStateService session)
    {
        _localDb = localDb;
        _api = api;
        _session = session;
    }

    public async Task<string> EnfileirarReservaAsync(
        LocalEspacoItem espaco,
        FinalidadeUsoEspaco finalidade,
        DateTime inicio,
        DateTime fim,
        string observacoes)
    {
        var reservaId = Guid.NewGuid();
        var itemLocal = new LocalReservaItem
        {
            Id = reservaId.ToString(),
            EspacoId = espaco.Id,
            EspacoNome = espaco.Nome,
            AssociadoNome = _session.Nome,
            FinalidadeUso = finalidade.ToString(),
            DataHoraInicio = inicio,
            DataHoraFim = fim,
            Status = "Pendente (Offline)",
            Observacoes = observacoes,
            CriadoOffline = true,
            PendenteSincronizacao = true
        };

        await _localDb.SaveReservaLocalAsync(itemLocal);

        var dto = new CriarReservaRequest(
            reservaId,
            Guid.Parse(espaco.Id),
            _session.ClubeId,
            _session.AssociadoId,
            finalidade,
            inicio,
            fim,
            observacoes,
            CriadoOffline: true);

        await _localDb.AddOutboxAsync("RESERVA", JsonSerializer.Serialize(dto, JsonOpts));
        await AtualizarContagemOutboxAsync();

        if (_session.EstaConectado && !string.IsNullOrWhiteSpace(_session.Token))
        {
            return await SincronizarAgoraAsync();
        }

        return "Reserva salva no SQLite local (Outbox). Sera enviada ao ficar online.";
    }

    public async Task<string> EnfileirarSolicitacaoAsync(TipoSolicitacao tipo, string assunto, string descricao)
    {
        var sacId = Guid.NewGuid();
        await _localDb.SaveSolicitacaoLocalAsync(new LocalSolicitacaoItem
        {
            Id = sacId.ToString(),
            Tipo = tipo.ToString(),
            Assunto = assunto,
            Descricao = descricao,
            Status = "Aberta (Offline)",
            DataCriacao = DateTime.UtcNow,
            PendenteSincronizacao = true
        });

        var dto = new CriarSolicitacaoRequest(sacId, _session.ClubeId, _session.AssociadoId, tipo, assunto, descricao);
        await _localDb.AddOutboxAsync("SOLICITACAO", JsonSerializer.Serialize(dto, JsonOpts));
        await AtualizarContagemOutboxAsync();

        if (_session.EstaConectado && !string.IsNullOrWhiteSpace(_session.Token))
        {
            return await SincronizarAgoraAsync();
        }

        return "Registro salvo no SQLite local (Outbox).";
    }

    public async Task<string> EnfileirarInscricaoEventoAsync(string eventoId, int acompanhantes)
    {
        await _localDb.MarkEventoInscritoAsync(eventoId);

        var payload = JsonSerializer.Serialize(new
        {
            EventoId = Guid.Parse(eventoId),
            AssociadoId = _session.AssociadoId,
            QuantidadeAcompanhantes = acompanhantes
        }, JsonOpts);

        await _localDb.AddOutboxAsync("INSCRICAO_EVENTO", payload);
        await AtualizarContagemOutboxAsync();

        if (_session.EstaConectado && !string.IsNullOrWhiteSpace(_session.Token))
        {
            return await SincronizarAgoraAsync();
        }

        return "Confirmacao salva localmente (Outbox).";
    }

    public async Task<string> SincronizarAgoraAsync()
    {
        if (!_session.EstaConectado)
        {
            await AtualizarContagemOutboxAsync();
            return "Modo Offline ativo. Mantenha os dados no SQLite local e sincronize quando conectar.";
        }

        if (string.IsNullOrWhiteSpace(_session.Token))
        {
            return "Autentique-se com a API para sincronizar com o servidor.";
        }

        try
        {
            var pendentes = await _localDb.GetPendingOutboxAsync();
            var mensagens = new List<string>();

            if (pendentes.Count > 0)
            {
                var pushReq = new SyncPushRequest(
                    _session.ClubeId,
                    _session.AssociadoId,
                    pendentes.Select(p => new SyncMutationDto(
                        Guid.Parse(p.OutboxId),
                        p.TipoOperacao,
                        p.PayloadJson,
                        p.CriadoEmUtc)).ToList());

                var pushResp = await _api.PushSyncAsync(pushReq);
                if (pushResp is not null)
                {
                    foreach (var r in pushResp.Resultados)
                    {
                        await _localDb.MarkOutboxSyncedAsync(
                            r.OutboxId.ToString(),
                            r.Sucesso,
                            r.Sucesso ? null : r.Mensagem);

                        if (r.StatusResultante == "Conflito")
                        {
                            mensagens.Add("Aviso: Uma reserva offline entrou em CONFLITO no servidor.");
                        }
                    }
                    mensagens.Insert(0, $"{pushResp.Resultados.Count(x => x.Sucesso)} item(ns) enviado(s).");
                }
            }

            var snapshot = await _api.PullSyncAsync(_session.ClubeId, _session.AssociadoId);
            if (snapshot is not null)
            {
                await _localDb.ReplaceSnapshotAsync(snapshot);
                _session.AtualizarPerfilDoPull(snapshot.Associado, snapshot.Clube);
                _session.UltimaSincronizacaoUtc = DateTime.UtcNow;
                mensagens.Add("Dados atualizados da API.");
            }

            await AtualizarContagemOutboxAsync();
            return mensagens.Count > 0 ? string.Join(" ", mensagens) : "Sincronizacao concluida.";
        }
        catch (Exception ex)
        {
            return $"API indisponivel ({ex.Message}). Operando em modo Offline local.";
        }
    }

    public async Task AtualizarContagemOutboxAsync()
    {
        _session.PendenciasOutbox = await _localDb.GetPendingOutboxCountAsync();
        _session.NotifyStateChanged();
    }
}
