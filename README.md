# Projeto Clubes — Sistema Acadêmico Multi-Clube (.NET 10)

Sistema completo de gestão multi-clube 100% em **C# (.NET 10)** com **Clean Architecture**, banco **SQLite local** (sem Docker) e três aplicações integradas:

1. **API REST (`Clube.Api`)** — `http://localhost:5000` (JWT + Multi-Tenancy + Sync Push/Pull)
2. **Painel Web Administrativo (`Clube.WebAdmin`)** — `http://localhost:5100` (Blazor Server Interactive)
3. **Aplicativo Móvel Offline-First (`Clube.Mobile`)** — .NET MAUI XAML/MVVM com `sqlite-net-pcl` + padrão Outbox

---

## Estrutura da Solution (`ProjetoClube.sln`)

```text
ProjetoClube/
├── ProjetoClube.sln
├── GEMINI.md
├── ARCHITECTURE.md
├── MEMORY.md
├── src/
│   ├── Clube.Domain/          # Entidades, Enums e Regras de Domínio (voto, elegibilidade, reservas)
│   ├── Clube.Shared/          # DTOs, Contratos de Sync e CpfValidator (Módulo 11)
│   ├── Clube.Application/     # Interfaces de Serviços, ITenantContext e PixPayloadGenerator (EMV CRC16)
│   ├── Clube.Infrastructure/  # ClubeDbContext (Global Query Filters), DbInitializer, JWT, BCrypt, Serviços
│   ├── Clube.Api/             # Web API REST (porta 5000)
│   ├── Clube.WebAdmin/        # Blazor Server (porta 5100)
│   └── Clube.Mobile/          # App .NET MAUI Offline-First (Windows Desktop & Android)
└── tests/
    ├── Clube.Domain.Tests/
    ├── Clube.Application.Tests/
    └── Clube.Api.IntegrationTests/
```

---

## Como Executar Localmente (Sem Docker)

### 1. Rodar Todos os Testes Automatizados
```powershell
dotnet test ProjetoClube.sln
```

### 2. Iniciar a API REST (`http://localhost:5000`)
```powershell
dotnet run --project src/Clube.Api/Clube.Api.csproj
```
- O banco SQLite local `app_clubes.db` é criado e populado automaticamente na raiz pelo `DbInitializer`.

### 3. Iniciar o Painel WebAdmin (`http://localhost:5100`)
Em outro terminal:
```powershell
dotnet run --project src/Clube.WebAdmin/Clube.WebAdmin.csproj
```
Acesse `http://localhost:5100` no navegador. O topo da tela oferece botões de **Demo Rápida** (`SuperAdmin`, `Pres. Foz`, `Tesoureiro Foz`, `Pres. Guaíra`) além do login por CPF e senha.

### 4. Iniciar o Aplicativo Móvel (`Clube.Mobile`)
No Windows (WinUI 3):
```powershell
dotnet build src/Clube.Mobile/Clube.Mobile.csproj -f net10.0-windows10.0.19041.0
dotnet run --project src/Clube.Mobile/Clube.Mobile.csproj -f net10.0-windows10.0.19041.0
```
*(Obs.: Para compilar o target `net10.0-android` no Windows via `aapt2`, mantenha o repositório em um caminho sem acentos e passe `-p:EnableAndroidBuild=true`.)*

---

## Credenciais de Demonstração (`DbInitializer`)

| Perfil | Clube | CPF | Senha |
| :--- | :--- | :--- | :--- |
| **SuperAdmin** | Todos (Federação) | `000.000.000-00` | `Admin@123` |
| **Presidente (Carlos Kaneko)** | Rotary Foz Ponte | `111.111.111-11` | `User@123` |
| **Tesoureiro** | Rotary Foz Ponte | `222.222.222-22` | `User@123` |
| **Sócio Fundador (com voto)** | Rotary Foz Ponte | `333.333.333-33` | `User@123` |
| **Sócio Convidado (sem voto)** | Rotary Foz Ponte | `444.444.444-44` | `User@123` |
| **Simpatizante (sem voto)** | Rotary Foz Ponte | `555.555.555-55` | `User@123` |
| **Presidente Guaíra** | Iate Clube Guaíra | `666.666.666-66` | `User@123` |
