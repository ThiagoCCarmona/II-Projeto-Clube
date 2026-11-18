using Clube.Mobile.Models;
using Clube.Shared.DTOs;
using SQLite;

namespace Clube.Mobile.Services;

/// <summary>
/// Banco SQLite local embutido (sqlite-net-pcl) para cache offline-first e fila Outbox.
/// </summary>
public class LocalDatabaseService
{
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _conn;

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_conn is not null)
        {
            return _conn;
        }

        await _initLock.WaitAsync();
        try
        {
            if (_conn is null)
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "clube_mobile_v1.db");
                _conn = new SQLiteAsyncConnection(dbPath);
                await _conn.CreateTableAsync<LocalEventoItem>();
                await _conn.CreateTableAsync<LocalEspacoItem>();
                await _conn.CreateTableAsync<LocalReservaItem>();
                await _conn.CreateTableAsync<LocalMensalidadeItem>();
                await _conn.CreateTableAsync<LocalSolicitacaoItem>();
                await _conn.CreateTableAsync<LocalDependenteItem>();
                await _conn.CreateTableAsync<OutboxMutationItem>();
                await SeedOfflineFallbackAsync(_conn);
            }
        }
        finally
        {
            _initLock.Release();
        }

        return _conn;
    }

    private static async Task SeedOfflineFallbackAsync(SQLiteAsyncConnection conn)
    {
        if (await conn.Table<LocalEspacoItem>().CountAsync() > 0)
        {
            return;
        }

        var clubeId = SessionStateService.DefaultClubeFozId.ToString();

        await conn.InsertAllAsync(new[]
        {
            new LocalEspacoItem
            {
                Id = "e1000000-0000-0000-0000-000000000001",
                ClubeId = clubeId,
                Nome = "Salao Social Principal",
                Capacidade = 220,
                Descricao = "Salao climatizado com cozinha industrial e palco.",
                TaxaReserva = 350.00m
            },
            new LocalEspacoItem
            {
                Id = "e1000000-0000-0000-0000-000000000002",
                ClubeId = clubeId,
                Nome = "Quiosque Pesca & Churrasco",
                Capacidade = 50,
                Descricao = "Area gourmet coberta proxima ao lago.",
                TaxaReserva = 120.00m
            }
        });

        await conn.InsertAllAsync(new[]
        {
            new LocalEventoItem
            {
                Id = "f1000000-0000-0000-0000-000000000001",
                ClubeId = clubeId,
                Titulo = "Almoco Dia das Maes",
                Descricao = "Almoco festivo com musica ao vivo.",
                TipoEvento = "AlmocoDiaDasMaes",
                PublicoAlvo = "AbertoAoPublico",
                DataInicio = new DateTime(2027, 5, 9, 11, 30, 0),
                DataFim = new DateTime(2027, 5, 9, 16, 0, 0),
                Local = "Salao Social Principal",
                ValorIngresso = 65.00m
            },
            new LocalEventoItem
            {
                Id = "f1000000-0000-0000-0000-000000000004",
                ClubeId = clubeId,
                Titulo = "Feijoada Beneficente",
                Descricao = "Feijoada tradicional com preparo continuo.",
                TipoEvento = "FeijoadaBeneficente",
                PublicoAlvo = "AbertoAoPublico",
                DataInicio = new DateTime(2027, 10, 31, 11, 0, 0),
                DataFim = new DateTime(2027, 11, 1, 17, 0, 0),
                Local = "Salao Social Principal",
                ValorIngresso = 70.00m
            }
        });
    }

    public async Task<List<LocalEventoItem>> GetEventosAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<LocalEventoItem>().OrderBy(e => e.DataInicio).ToListAsync();
    }

    public async Task<List<LocalEspacoItem>> GetEspacosAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<LocalEspacoItem>().OrderBy(e => e.Nome).ToListAsync();
    }

    public async Task<List<LocalReservaItem>> GetReservasAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<LocalReservaItem>().OrderBy(r => r.DataHoraInicio).ToListAsync();
    }

    public async Task<List<LocalMensalidadeItem>> GetMensalidadesAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<LocalMensalidadeItem>().OrderByDescending(m => m.DataVencimento).ToListAsync();
    }

    public async Task<List<LocalSolicitacaoItem>> GetSolicitacoesAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<LocalSolicitacaoItem>().OrderByDescending(s => s.DataCriacao).ToListAsync();
    }

    public async Task<List<LocalDependenteItem>> GetDependentesAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<LocalDependenteItem>().OrderBy(d => d.Nome).ToListAsync();
    }

    public async Task SaveReservaLocalAsync(LocalReservaItem item)
    {
        var db = await GetConnectionAsync();
        await db.InsertOrReplaceAsync(item);
    }

    public async Task SaveSolicitacaoLocalAsync(LocalSolicitacaoItem item)
    {
        var db = await GetConnectionAsync();
        await db.InsertOrReplaceAsync(item);
    }

    public async Task MarkEventoInscritoAsync(string eventoId)
    {
        var db = await GetConnectionAsync();
        var item = await db.FindAsync<LocalEventoItem>(eventoId);
        if (item is not null)
        {
            item.UsuarioInscrito = true;
            item.TotalConfirmados++;
            await db.UpdateAsync(item);
        }
    }

    public async Task AddDependenteLocalAsync(LocalDependenteItem item)
    {
        var db = await GetConnectionAsync();
        await db.InsertOrReplaceAsync(item);
    }

    public async Task AddOutboxAsync(string tipoOperacao, string payloadJson)
    {
        var db = await GetConnectionAsync();
        await db.InsertAsync(new OutboxMutationItem
        {
            OutboxId = Guid.NewGuid().ToString(),
            TipoOperacao = tipoOperacao,
            PayloadJson = payloadJson,
            CriadoEmUtc = DateTime.UtcNow,
            Sincronizado = false
        });
    }

    public async Task<List<OutboxMutationItem>> GetPendingOutboxAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<OutboxMutationItem>()
            .Where(o => !o.Sincronizado)
            .OrderBy(o => o.CriadoEmUtc)
            .ToListAsync();
    }

    public async Task<int> GetPendingOutboxCountAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<OutboxMutationItem>().Where(o => !o.Sincronizado).CountAsync();
    }

    public async Task MarkOutboxSyncedAsync(string outboxId, bool sucesso, string? erro = null)
    {
        var db = await GetConnectionAsync();
        var item = await db.FindAsync<OutboxMutationItem>(outboxId);
        if (item is not null)
        {
            item.Sincronizado = sucesso;
            item.UltimoErro = erro;
            await db.UpdateAsync(item);
        }
    }

    public async Task ReplaceSnapshotAsync(SyncPullResponse snapshot)
    {
        var db = await GetConnectionAsync();

        await db.DeleteAllAsync<LocalEspacoItem>();
        await db.InsertAllAsync(snapshot.Espacos.Select(e => new LocalEspacoItem
        {
            Id = e.Id.ToString(),
            ClubeId = e.ClubeId.ToString(),
            Nome = e.Nome,
            Capacidade = e.Capacidade,
            Descricao = e.Descricao,
            TaxaReserva = e.TaxaReserva
        }));

        await db.DeleteAllAsync<LocalEventoItem>();
        await db.InsertAllAsync(snapshot.Eventos.Select(e => new LocalEventoItem
        {
            Id = e.Id.ToString(),
            ClubeId = e.ClubeId.ToString(),
            Titulo = e.Titulo,
            Descricao = e.Descricao,
            TipoEvento = e.TipoEvento.ToString(),
            PublicoAlvo = e.PublicoAlvo.ToString(),
            DataInicio = e.DataInicio,
            DataFim = e.DataFim,
            Local = e.Local,
            ValorIngresso = e.ValorIngresso,
            TotalConfirmados = e.TotalConfirmados
        }));

        // Preserva itens pendentes no outbox enquanto atualiza os registros confirmados pelo servidor
        var pendentesReservas = (await db.Table<LocalReservaItem>()
            .Where(r => r.PendenteSincronizacao)
            .ToListAsync()).ToDictionary(r => r.Id);

        await db.DeleteAllAsync<LocalReservaItem>();
        var novasReservas = snapshot.Reservas.Select(r => new LocalReservaItem
        {
            Id = r.Id.ToString(),
            EspacoId = r.EspacoId.ToString(),
            EspacoNome = r.EspacoNome,
            AssociadoNome = r.AssociadoNome,
            FinalidadeUso = r.FinalidadeUso.ToString(),
            DataHoraInicio = r.DataHoraInicio,
            DataHoraFim = r.DataHoraFim,
            Status = r.Status.ToString(),
            Observacoes = r.Observacoes,
            CriadoOffline = r.CriadoOffline,
            PendenteSincronizacao = false
        }).ToList();

        foreach (var pendente in pendentesReservas.Values)
        {
            if (novasReservas.All(n => n.Id != pendente.Id))
            {
                novasReservas.Add(pendente);
            }
        }
        await db.InsertAllAsync(novasReservas);

        await db.DeleteAllAsync<LocalMensalidadeItem>();
        await db.InsertAllAsync(snapshot.Mensalidades.Select(m => new LocalMensalidadeItem
        {
            Id = m.Id.ToString(),
            Competencia = m.Competencia,
            Valor = m.Valor,
            DataVencimento = m.DataVencimento,
            DataPagamento = m.DataPagamento,
            Status = m.Status.ToString(),
            CodigoPixCopiaECola = m.CodigoPixCopiaECola,
            Observacoes = m.Observacoes
        }));

        await db.DeleteAllAsync<LocalSolicitacaoItem>();
        await db.InsertAllAsync(snapshot.Solicitacoes.Select(s => new LocalSolicitacaoItem
        {
            Id = s.Id.ToString(),
            Tipo = s.Tipo.ToString(),
            Assunto = s.Assunto,
            Descricao = s.Descricao,
            RespostaDiretoria = s.RespostaDiretoria,
            Status = s.Status.ToString(),
            DataCriacao = s.DataCriacao,
            PendenteSincronizacao = false
        }));

        if (snapshot.Associado is not null)
        {
            await db.DeleteAllAsync<LocalDependenteItem>();
            await db.InsertAllAsync(snapshot.Associado.Dependentes.Select(d => new LocalDependenteItem
            {
                Id = d.Id.ToString(),
                Nome = d.Nome,
                CPF = d.CPF,
                GrauParentesco = d.GrauParentesco.ToString(),
                Email = d.Email
            }));
        }
    }
}
