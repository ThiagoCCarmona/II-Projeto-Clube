using System.Net.Http.Headers;
using System.Net.Http.Json;
using Clube.Shared.DTOs;

namespace Clube.Mobile.Services;

/// <summary>
/// Cliente HTTP REST da API local tratando automaticamente localhost (Windows) e 10.0.2.2 (Emulador Android).
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly SessionStateService _session;

    public ApiClient(SessionStateService session)
    {
        _session = session;
        var baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5000"
            : "http://localhost:5000";

        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    private void AplicarBearer()
    {
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(_session.Token)
            ? null
            : new AuthenticationHeaderValue("Bearer", _session.Token);
    }

    public async Task<List<ClubeDto>> ListarClubesAsync()
    {
        var resp = await _http.GetFromJsonAsync<List<ClubeDto>>("/api/clubes");
        return resp ?? [];
    }

    public async Task<LoginResponse?> LoginAsync(string cpf, string senha)
    {
        var resp = await _http.PostAsJsonAsync("/api/auth/login", new LoginRequest(cpf, senha));
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        return await resp.Content.ReadFromJsonAsync<LoginResponse>();
    }

    public async Task<(bool Sucesso, string Mensagem)> RegistroPublicoAsync(RegistroPublicoRequest request)
    {
        var resp = await _http.PostAsJsonAsync("/api/auth/registro-publico", request);
        if (resp.IsSuccessStatusCode)
        {
            return (true, "Cadastro enviado! Aguardando aprovacao do Presidente do clube.");
        }

        var erro = await resp.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(erro) ? $"Erro HTTP {(int)resp.StatusCode}" : erro);
    }

    public async Task<SyncPushResponse?> PushSyncAsync(SyncPushRequest request)
    {
        AplicarBearer();
        var resp = await _http.PostAsJsonAsync("/api/sync/push", request);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<SyncPushResponse>();
    }

    public async Task<SyncPullResponse?> PullSyncAsync(Guid clubeId, Guid? associadoId)
    {
        AplicarBearer();
        var url = $"/api/sync/pull?clubeId={clubeId}&associadoId={associadoId}";
        return await _http.GetFromJsonAsync<SyncPullResponse>(url);
    }

    public async Task<DependenteDto?> AdicionarDependenteAsync(Guid associadoId, CriarDependenteRequest request)
    {
        AplicarBearer();
        var resp = await _http.PostAsJsonAsync($"/api/associados/{associadoId}/dependentes", request);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<DependenteDto>();
    }
}
