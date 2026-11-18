# GEMINI.md — Regras e Diretrizes do Projeto Clubes

Este arquivo define as instruções obrigatórias que todo agente de IA deve seguir ao trabalhar neste repositório.

---

## 1. Visão Geral e Stack Tecnológica
- **Natureza do Projeto:** Sistema acadêmico de gestão multi-clube ("Projeto Clubes").
- **Linguagem e Plataforma:** **100% C# (.NET 10)**.
- **Execução:** **100% Local** (via `dotnet run` / Visual Studio). **Não criar `Dockerfile`, `docker-compose.yml` ou scripts de container.**
- **Banco de Dados:** SQLite local compartilhado entre API e WebAdmin (`app_clubes.db`) e SQLite embutido no app móvel (`sqlite-net-pcl`).
- **Front-end Web:** Blazor Server (`Clube.WebAdmin`).
- **App Móvel:** .NET MAUI Nativo com XAML + MVVM (`Clube.Mobile`) visando **Windows Desktop (WinUI 3)** e **Android**.

---

## 2. Leitura Obrigatória Antes de Codar
Antes de propor planos ou editar arquivos:
1. Leia `ARCHITECTURE.md` para entender os 7 projetos, entidades, isolamento multi-tenant e contratos.
2. Leia `MEMORY.md` para não repetir erros já diagnosticados (portas, HTTPS local, bootstrap do MAUI no Windows e Shell DI).

---

## 3. Regras de Ouro (Invariantes do Projeto)
1. **Isolamento Multi-Tenant Estrito:**
   - Toda entidade vinculada a um clube possui `ClubeId`.
   - O `ClubeDbContext` deve aplicar **Global Query Filters** filtrando pelo `ITenantContext.CurrentClubeId`.
   - Apenas o perfil `SuperAdmin` (`CurrentClubeId == null`) pode visualizar dados de todos os clubes.
2. **Direito a Voto:**
   - Somente `TipoAssociado.SocioFundadorComVoto` com `StatusAprovacao.Aprovado` tem direito a voto (`TemDireitoAVoto() == true`).
   - `SocioConvidadoSemVoto` e `Simpatizante` **nunca** têm direito a voto.
3. **Reservas Contínuas e Conflitos:**
   - Reservas aceitam intervalo contínuo `DataHoraInicio` até `DataHoraFim` (podendo cruzar múltiplos dias, ex.: `31/10 08:00` a `01/11 18:00`).
   - Sobreposição de horários (`inicioA < fimB && fimA > inicioB`) no mesmo `EspacoId` deve ser detectada automaticamente.
4. **Execução HTTP Local Simples (Sem Travas de HTTPS):**
   - `Clube.Api` deve rodar em `http://localhost:5000`.
   - `Clube.WebAdmin` deve rodar em `http://localhost:5100`.
   - **NÃO** chamar `app.UseHttpsRedirection()` em `Clube.WebAdmin` ou `Clube.Api` no modo local HTTP.
5. **Estabilidade do .NET MAUI no Windows:**
   - Em `Clube.Mobile.csproj`, incluir sempre `<WindowsAppSDKSelfContained Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">true</WindowsAppSDKSelfContained>`.
   - Toda `ContentPage` registrada em `AppShell.xaml` deve possuir um **construtor sem parâmetros** com fallback de resolução via `IPlatformApplication.Current?.Services`.

---

## 4. Boas Práticas de Código e UI/UX
- **Clean Architecture:** `Domain` não depende de ninguém; `Shared` depende só de `Domain` (ou é independente); `Application` depende de `Domain` e `Shared`; `Infrastructure` implementa `Application` e `Domain`.
- **Sem Duplicação:** Extraia helpers compartilhados (ex.: validador de CPF, gerador PIX EMV CRC16-CCITT).
- **Documentação:** Comente o *porquê* de regras não óbvias e adicione XML docs nas interfaces públicas de domínio/serviço.
- **UI/UX:**
  - WebAdmin com navegação clara por perfil (SuperAdmin, Presidente, Tesoureiro), badges de status, seletor rápido de demonstração para apresentação acadêmica e feedback visual imediato.
  - App Mobile com indicador visível de modo Online/Offline, botões de ação claros e preenchimento rápido de credenciais de demonstração na tela de login.

---

## 5. Fluxo de Trabalho e Verificação Obrigatória
1. **TDD:** Escreva testes unitários antes ou junto com as regras de negócio (`Clube.Domain.Tests`, `Clube.Application.Tests`, `Clube.Api.IntegrationTests`).
2. **Nada de afirmações sem evidência:** Nunca diga que compilou ou que os testes passaram sem executar:
   ```powershell
   dotnet test ProjetoClube.sln
   ```
3. **Atualize o `MEMORY.md`:** Sempre que descobrir uma nova restrição de ambiente, correção de bug ou decisão importante, registre no `MEMORY.md`.
