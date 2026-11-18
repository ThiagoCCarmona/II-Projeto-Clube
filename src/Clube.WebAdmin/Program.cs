using Clube.Infrastructure;
using Clube.Infrastructure.Persistence;
using Clube.WebAdmin.Components;
using Clube.WebAdmin.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? DependencyInjection.ResolveLocalDatabasePath(builder.Environment.ContentRootPath);

builder.Services.AddClubeInfrastructure(connectionString);
builder.Services.AddScoped<AdminSessionState>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClubeDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DbInitializer.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// Regra crítica (MEMORY.md): NÃO usar UseHttpsRedirection em modo HTTP local (:5100).
// Garantir UseStaticFiles() antes de MapStaticAssets() para CSS/JS locais.
app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
