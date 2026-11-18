using Clube.Application.Interfaces;
using Clube.Infrastructure.Persistence;
using Clube.Infrastructure.Security;
using Clube.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clube.Infrastructure;

/// <summary>
/// Extensão de registro da camada de infraestrutura e serviços de aplicação no container DI.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddClubeInfrastructure(
        this IServiceCollection services,
        string sqliteConnectionString)
    {
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddDbContext<ClubeDbContext>(opt => opt.UseSqlite(sqliteConnectionString));

        services.AddSingleton<JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClubeService, ClubeService>();
        services.AddScoped<IAssociadoService, AssociadoService>();
        services.AddScoped<IEventoService, EventoService>();
        services.AddScoped<IReservaService, ReservaService>();
        services.AddScoped<IMensalidadeService, MensalidadeService>();
        services.AddScoped<ISolicitacaoService, SolicitacaoService>();
        services.AddScoped<ISyncService, SyncService>();

        return services;
    }

    /// <summary>
    /// Resolve o caminho absoluto padronizado para o banco SQLite local compartilhado na raiz da solution.
    /// </summary>
    public static string ResolveLocalDatabasePath(string contentRootPath)
    {
        var dir = new DirectoryInfo(contentRootPath);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ProjetoClube.sln")))
        {
            dir = dir.Parent;
        }

        var rootDir = dir?.FullName ?? contentRootPath;
        var dbPath = Path.Combine(rootDir, "app_clubes.db");
        return $"Data Source={dbPath}";
    }
}
