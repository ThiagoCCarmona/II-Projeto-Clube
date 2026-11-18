# Plano de Implementação Completo — Projeto Clubes (.NET 10)

## Visão Geral e Diretrizes Obrigatórias (`GEMINI.md`, `ARCHITECTURE.md`, `MEMORY.md`)
O sistema **Projeto Clubes** deve ser construído 100% em **C# (.NET 10)** com **Clean Architecture**, execução **100% local (sem Docker)** e banco **SQLite local** (`app_clubes.db` na raiz da solution + `sqlite-net-pcl` no app MAUI).

### Invariantes e Prevenção de Armadilhas (`MEMORY.md`)
1. **Apenas um arquivo de solução (`.sln`):** No .NET 10, `dotnet new sln` gera `.slnx` por padrão. Sempre criar com `dotnet new sln -n ProjetoClube --format sln` e garantir que não exista `.slnx` na raiz (Erro 5 do `MEMORY.md`).
2. **Sem `UseHttpsRedirection()` em HTTP local:** `Clube.Api` roda em `http://localhost:5000` e `Clube.WebAdmin` roda em `http://localhost:5100`. Nunca chamar `app.UseHttpsRedirection()` em nenhum dos dois `Program.cs`. No `Clube.WebAdmin`, chamar `app.UseStaticFiles();` antes de `app.MapStaticAssets();` (Erro 1 do `MEMORY.md`).
3. **Estabilidade do `.NET MAUI` no Windows (WinUI 3):** Em `src/Clube.Mobile/Clube.Mobile.csproj`, incluir obrigatoriamente:
   ```xml
   <WindowsPackageType>None</WindowsPackageType>
   <WindowsAppSDKSelfContained Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">true</WindowsAppSDKSelfContained>
   ```
   (Erro 2 do `MEMORY.md`).
4. **Construtores sem parâmetros nas `ContentPage`s do MAUI:** Toda página registrada no `AppShell.xaml` (`LoginPage`, `RegisterPage`, `EventosPage`, `ReservasPage`, `MensalidadesPage`, `SacPage`, `PerfilPage`) deve ter o construtor injetado `public MinhaPage(MinhaViewModel vm)` **E** um construtor sem parâmetros `: this(AppServiceHelper.GetRequiredService<MinhaViewModel>())` que usa `IPlatformApplication.Current?.Services` (Erro 3 do `MEMORY.md`).
5. **URL da API no Android vs Windows:** Em `Clube.Mobile/Services/ApiClient.cs`, usar `DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5000" : "http://localhost:5000"` (Erro 4 do `MEMORY.md`).

---

## Fases Incrementais (com TDD e `dotnet test` em cada fase)
- **Fase 1:** `ProjetoClube.sln` + `Clube.Domain` + `Clube.Shared` + `Clube.Domain.Tests`
- **Fase 2:** `Clube.Application` + `Clube.Infrastructure` + `Clube.Application.Tests` + `Clube.Api` + `Clube.Api.IntegrationTests`
- **Fase 3:** `Clube.WebAdmin` (Blazor Server em `http://localhost:5100`)
- **Fase 4:** `Clube.Mobile` (.NET 10 MAUI XAML/MVVM Offline-First) + verificação final + atualização do `MEMORY.md` e `README.md`
