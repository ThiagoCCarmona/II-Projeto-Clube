using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Clube.Domain.Enums;
using Clube.Infrastructure.Persistence;
using Clube.Shared.DTOs;
using Clube.Shared.Validators;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Clube.Api.IntegrationTests;

public class CustomApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(),
        $"projeto_clube_test_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ClubeDbContext>) ||
                            d.ServiceType == typeof(IDbContextOptionsConfiguration<ClubeDbContext>))
                .ToList();

            foreach (var d in descriptors)
            {
                services.Remove(d);
            }

            services.AddDbContext<ClubeDbContext>(opt => opt.UseSqlite($"Data Source={_dbPath}"));
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        if (File.Exists(_dbPath))
        {
            try { File.Delete(_dbPath); } catch { /* ignore sqlite lock on test cleanup */ }
        }
    }
}

public class ApiIntegrationTests : IClassFixture<CustomApiFactory>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(CustomApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<LoginResponse> AutenticarAsync(string cpf, string senha)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(cpf, senha));
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        return login;
    }

    [Fact]
    public async Task GetClubes_DeveRetornarClubesIniciaisSemAutenticacao()
    {
        var response = await _client.GetAsync("/api/clubes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var clubes = await response.Content.ReadFromJsonAsync<List<ClubeDto>>();
        Assert.NotNull(clubes);
        Assert.Contains(clubes, c => c.Id == DbInitializer.ClubeFozId);
        Assert.Contains(clubes, c => c.Id == DbInitializer.ClubeGuairaId);
    }

    [Fact]
    public async Task LoginETenantIsolation_PresidenteFozDeveVerApenasAssociadosDeFoz()
    {
        var loginFoz = await AutenticarAsync("111.111.111-11", "User@123");
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/associados");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginFoz.Token);

        var resp = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var associados = await resp.Content.ReadFromJsonAsync<List<AssociadoDto>>();
        Assert.NotNull(associados);
        Assert.NotEmpty(associados);
        Assert.All(associados, a => Assert.Equal(DbInitializer.ClubeFozId, a.ClubeId));
        Assert.DoesNotContain(associados, a => a.ClubeId == DbInitializer.ClubeGuairaId);
    }

    [Fact]
    public async Task SuperAdmin_DeveVisualizarAssociadosDeTodosOsClubes()
    {
        var loginAdmin = await AutenticarAsync("000.000.000-00", "Admin@123");
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/associados");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginAdmin.Token);

        var resp = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var associados = await resp.Content.ReadFromJsonAsync<List<AssociadoDto>>();
        Assert.NotNull(associados);
        Assert.Contains(associados, a => a.ClubeId == DbInitializer.ClubeFozId);
        Assert.Contains(associados, a => a.ClubeId == DbInitializer.ClubeGuairaId);
    }

    [Fact]
    public async Task FluxoCompleto_RegistroPublicoAprovacaoSyncPushPullEMensalidade()
    {
        var cpfNovo = CpfValidator.GerarCpfValido(778899);
        var regResp = await _client.PostAsJsonAsync("/api/auth/registro-publico", new RegistroPublicoRequest(
            DbInitializer.ClubeFozId,
            "Novo Socio E2E",
            cpfNovo,
            "novo.e2e@clube.local",
            "(45) 99988-7766",
            "Senha@123",
            TipoAssociado.SocioFundadorComVoto,
            new DateTime(1991, 6, 10),
            "Masculino",
            "Solteiro",
            "Rua E2E, 100",
            "Centro",
            "Foz do Iguacu",
            "PR",
            "Pesca, Comida"));

        Assert.Equal(HttpStatusCode.Created, regResp.StatusCode);
        var criado = await regResp.Content.ReadFromJsonAsync<AssociadoDto>();
        Assert.NotNull(criado);
        Assert.Equal(StatusAprovacao.Pendente, criado.StatusAprovacao);
        Assert.False(criado.TemDireitoAVoto);

        // Presidente aprova novo associado
        var presLogin = await AutenticarAsync("111.111.111-11", "User@123");
        using var aprovarReq = new HttpRequestMessage(HttpMethod.Post, $"/api/associados/{criado.Id}/aprovar");
        aprovarReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", presLogin.Token);
        var aprovarResp = await _client.SendAsync(aprovarReq);
        Assert.Equal(HttpStatusCode.OK, aprovarResp.StatusCode);
        var aprovado = await aprovarResp.Content.ReadFromJsonAsync<AssociadoDto>();
        Assert.NotNull(aprovado);
        Assert.True(aprovado.TemDireitoAVoto);

        // Novo sócio faz login e envia reserva offline via /api/sync/push
        var socioLogin = await AutenticarAsync(cpfNovo, "Senha@123");
        Assert.True(socioLogin.TemDireitoAVoto);

        using var pullReq = new HttpRequestMessage(HttpMethod.Get, $"/api/sync/pull?clubeId={DbInitializer.ClubeFozId}&associadoId={criado.Id}");
        pullReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", socioLogin.Token);
        var pullResp = await _client.SendAsync(pullReq);
        Assert.Equal(HttpStatusCode.OK, pullResp.StatusCode);
        var snapshot = await pullResp.Content.ReadFromJsonAsync<SyncPullResponse>();
        Assert.NotNull(snapshot);
        Assert.NotEmpty(snapshot.Espacos);

        var espacoId = snapshot.Espacos.First().Id;
        var reservaId = Guid.NewGuid();
        var pushPayload = JsonSerializer.Serialize(new CriarReservaRequest(
            reservaId,
            espacoId,
            DbInitializer.ClubeFozId,
            criado.Id,
            FinalidadeUsoEspaco.Churrasco,
            new DateTime(2028, 7, 15, 9, 0, 0),
            new DateTime(2028, 7, 16, 17, 0, 0),
            "Reserva E2E Offline",
            CriadoOffline: true));

        using var pushReq = new HttpRequestMessage(HttpMethod.Post, "/api/sync/push")
        {
            Content = JsonContent.Create(new SyncPushRequest(
                DbInitializer.ClubeFozId,
                criado.Id,
                [new SyncMutationDto(Guid.NewGuid(), "RESERVA", pushPayload, DateTime.UtcNow)]))
        };
        pushReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", socioLogin.Token);
        var pushResp = await _client.SendAsync(pushReq);
        Assert.Equal(HttpStatusCode.OK, pushResp.StatusCode);
        var pushResult = await pushResp.Content.ReadFromJsonAsync<SyncPushResponse>();
        Assert.NotNull(pushResult);
        Assert.Single(pushResult.Resultados);
        Assert.True(pushResult.Resultados[0].Sucesso);
    }
}
