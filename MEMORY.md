# MEMORY.md — Memória Técnica e Lições Aprendidas

Este arquivo registra decisões de ambiente, bugs históricos já diagnosticados e soluções definitivas para que não se repitam na reconstrução do projeto.

---

## 1. Ambiente de Execução (100% Local — Sem Docker)
- **Decisão:** O projeto é acadêmico e deve rodar **exclusivamente local** com `dotnet run` e banco SQLite (`app_clubes.db`). **Não usar Docker.**
- **Portas Padronizadas:**
  - `Clube.Api`: `http://localhost:5000` (definido em `src/Clube.Api/Properties/launchSettings.json`)
  - `Clube.WebAdmin`: `http://localhost:5100` (definido em `src/Clube.WebAdmin/Properties/launchSettings.json`)

---

## 2. Armadilhas Conhecidas e Como Evitá-las

### ❌ Erro 1: WebAdmin não abre / trava no redirecionamento HTTPS
- **Sintoma:** `Failed to determine the https port for redirect.` e página não abre em `http://localhost:5100`.
- **Causa:** `app.UseHttpsRedirection()` no `Clube.WebAdmin/Program.cs` quando o perfil roda apenas em HTTP (`http://localhost:5100`).
- **Prevenção:**
  - **Não incluir** `app.UseHttpsRedirection()` em `Clube.WebAdmin/Program.cs` nem em `Clube.Api/Program.cs`.
  - Garantir `app.UseStaticFiles();` antes de `app.MapStaticAssets();` no WebAdmin.
  - Configurar `launchSettings.json` do WebAdmin com `"applicationUrl": "http://localhost:5100"` e da API com `"applicationUrl": "http://localhost:5000"`.

### ❌ Erro 2: Aplicativo .NET MAUI fecha imediatamente ao abrir no Windows
- **Sintoma:** O app compila sem erros, mas fecha silenciosamente antes de abrir a janela no Windows 10/11 quando executado fora do Visual Studio.
- **Causa:** Aplicativos WinUI 3 desempacotados (`<WindowsPackageType>None</WindowsPackageType>`) falham no bootstrapper nativo se o runtime do Windows App SDK não estiver instalado globalmente na máquina.
- **Prevenção:** Adicionar obrigatoriamente em `src/Clube.Mobile/Clube.Mobile.csproj`:
  ```xml
  <WindowsPackageType>None</WindowsPackageType>
  <WindowsAppSDKSelfContained Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">true</WindowsAppSDKSelfContained>
  ```

### ❌ Erro 3: Shell `DataTemplate` falha ao instanciar páginas do MAUI
- **Sintoma:** Crash ou falha silenciosa ao navegar ou abrir a `LoginPage` / abas do `AppShell`.
- **Causa:** `ShellContent` com `ContentTemplate="{DataTemplate views:LoginPage}"` tenta chamar um construtor **sem parâmetros** caso não seja resolvido diretamente pelo container em algum fluxo.
- **Prevenção:** Toda `ContentPage` (`LoginPage`, `RegisterPage`, `EventosPage`, `ReservasPage`, `MensalidadesPage`, `SacPage`, `PerfilPage`) deve ter:
  1. Um construtor principal recebendo sua `ViewModel` via DI.
  2. Um construtor sem parâmetros `: this(IPlatformApplication.Current?.Services?.GetService<MinhaViewModel>() ?? ...)` como fallback seguro.

### ❌ Erro 4: Conexão do App MAUI com a API no Emulador Android vs Windows
- **Causa:** No emulador Android, `localhost` aponta para o próprio dispositivo Android, não para o PC host.
- **Prevenção:** Em `Clube.Mobile/Services/ApiClient.cs`:
  ```csharp
  var baseUrl = DeviceInfo.Platform == DevicePlatform.Android
      ? "http://10.0.2.2:5000"
      : "http://localhost:5000";
  ```

### ❌ Erro 5: `dotnet test` na raiz com múltiplos arquivos de solução
- **Causa:** No .NET 10, `dotnet new sln` gera `.slnx` por padrão; se houver `.sln` e `.slnx` na raiz, `dotnet test` sem argumento falha com `MSB1011`.
- **Prevenção:** Criar sempre com `dotnet new sln -n ProjetoClube --format sln` mantendo apenas `ProjetoClube.sln` (e sempre executar `dotnet test ProjetoClube.sln`).

### ❌ Erro 6: Pacotes NuGet `11.0.0` incompatíveis ao adicionar sem versão em projetos `net10.0`
- **Sintoma:** `NU1202: O pacote ... 11.0.0 não é compatível com net10.0`.
- **Prevenção:** Ao instalar pacotes `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.AspNetCore.Authentication.JwtBearer` ou `Microsoft.AspNetCore.Mvc.Testing` em projetos `.NET 10`, fixar a versão compatível (ex.: `-v 10.0.11`).

### ❌ Erro 7: Testes de integração (`WebApplicationFactory`) tocando o banco `app_clubes.db` da raiz
- **Causa:** `builder.Configuration.GetConnectionString("DefaultConnection")` avaliado nos *top-level statements* do `Program.cs` executa antes do callback `ConfigureAppConfiguration` da `WebApplicationFactory`.
- **Prevenção:** No `CustomApiFactory`, remover as registrações `DbContextOptions<ClubeDbContext>` / `IDbContextOptionsConfiguration<ClubeDbContext>` em `ConfigureServices` e registrar `UseSqlite` apontando para arquivo temporário isolado em `Path.GetTempPath()`.

### ❌ Erro 8: Erro Android `APT2265` quando o workspace contém caracteres acentuados no caminho
- **Sintoma:** `error APT2265` no target `net10.0-android` quando a pasta do repositório possui acentos (`Ciencia da Computação - UDC\Programação Web Mobile`).
- **Prevenção:** Em `src/Clube.Mobile/Clube.Mobile.csproj`, manter `net10.0-windows10.0.19041.0` como target padrão no Windows e habilitar `net10.0-android` condicionalmente (`-p:EnableAndroidBuild=true` quando o projeto estiver em diretório apenas ASCII, além de `android:usesCleartextTraffic="true"` no `AndroidManifest.xml` para permitir `http://10.0.2.2:5000`).

---

## 3. Credenciais Padrão para Validação Rápida
- **SuperAdmin:** `000.000.000-00` / `Admin@123`
- **Presidente (Carlos Kaneko):** `111.111.111-11` / `User@123`
- **Tesoureiro:** `222.222.222-22` / `User@123`
- **Sócio Fundador (com voto):** `333.333.333-33` / `User@123`
- **Sócio Convidado (sem voto):** `444.444.444-44` / `User@123`
- **Simpatizante (sem voto):** `555.555.555-55` / `User@123`
