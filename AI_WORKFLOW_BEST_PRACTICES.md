# 🧠 Boas Práticas de Desenvolvimento Assistido por IA

Este guia reúne o fluxo profissional recomendado para conduzir o desenvolvimento do **Projeto Clubes** (ou qualquer sistema complexo) junto a um agente de IA, mantendo alta qualidade de código e evitando alucinações ou retrabalho.

---

## 1. Estrutura de Contexto Persistente na Raiz
Agentes de IA perdem contexto quando a conversa cresce ou quando uma nova sessão começa. Por isso, mantenha sempre estes 3 arquivos vivos na raiz do projeto:

1. **`GEMINI.md` (Constituição do Projeto):**
   - Contém regras imutáveis: stack, padrão arquitetural, portas, proibições (ex.: "não usar Docker", "não usar `UseHttpsRedirection` em HTTP local").
   - O agente lê esse arquivo automaticamente ou logo no início da sessão.
2. **`ARCHITECTURE.md` (Mapa Técnico):**
   - Descreve projetos, camadas, entidades, endpoints e fluxo de sincronização.
   - Evita que a IA crie classes duplicadas ou coloque regra de negócio dentro de Controllers ou Views.
3. **`MEMORY.md` (Diário de Aprendizado):**
   - Sempre que você e a IA resolverem um bug difícil (como o bootstrapper do MAUI no Windows ou conflito de porta), peça: *"Registre essa solução no MEMORY.md"*.
   - Na próxima sessão, a IA não cometerá o mesmo erro.

---

## 2. Desenvolvimento em Fases (Nunca Peça "Faça Tudo de Uma Vez")
Divida a construção em etapas pequenas e verificáveis:

- **Fase 1 — Design & Planejamento:** Alinhe entidades, enums e casos de uso antes de criar arquivos.
- **Fase 2 — Domínio + Contratos + Testes Unitários (`Clube.Domain` + `Clube.Shared` + `Clube.Domain.Tests`):**
  - Modele regras puras (ex.: `TemDireitoAVoto()`, `CpfValidator`) usando **TDD** (teste primeiro, código depois).
- **Fase 3 — Infraestrutura + Aplicação + Testes de Serviço (`Clube.Infrastructure` + `Clube.Application` + `Clube.Application.Tests`):**
  - Configure `ClubeDbContext` com filtro multi-tenant, `DbInitializer`, `PixPayloadGenerator`, `ReservaService` e `SyncService`.
- **Fase 4 — Web API + Testes de Integração (`Clube.Api` + `Clube.Api.IntegrationTests`):**
  - Exponha controllers REST, JWT e teste endpoints com `WebApplicationFactory`.
- **Fase 5 — Painel WebAdmin (`Clube.WebAdmin`):**
  - Construa as telas Blazor por perfil (SuperAdmin, Presidente, Tesoureiro) e valide no navegador em `http://localhost:5100`.
- **Fase 6 — App Móvel Offline-First (`Clube.Mobile`):**
  - Configure `LocalDatabaseService`, `MobileSyncService`, ViewModels e Views XAML, validando compilação e inicialização no Windows/Android.

---

## 3. Regras de Ouro ao Interagir com o Agente

1. **Exija TDD para Regras Críticas:**
   - Ex.: *"Antes de implementar o cálculo de conflito de reservas e o gerador de PIX, escreva os testes unitários em `Clube.Application.Tests`."*
2. **Exija Evidência Real (`Verification Before Completion`):**
   - Nunca aceite apenas "tudo pronto e funcionando". Peça para o agente rodar `dotnet test ProjetoClube.sln`, subir `Clube.Api` e `Clube.WebAdmin` e testar os endpoints/páginas HTTP reais.
3. **Compacte ou Reinicie Sessões Longas:**
   - Quando terminar uma fase inteira (ex.: Backend pronto e testado), atualize o `MEMORY.md` e, se a conversa ficar muito extensa, inicie uma nova conversa apontando para `GEMINI.md` e `ARCHITECTURE.md`.
4. **Mantenha um Seletor/Atalho de Demonstração Acadêmica:**
   - Para apresentações de trabalho acadêmico, ter seed automático (`DbInitializer`) e botões de preenchimento/troca rápida de perfil no WebAdmin e Mobile economiza tempo e evita erros ao vivo.
5. **Commits Pequenos por Fase:**
   - Faça `git commit` ao final de cada fase com testes verdes (`feat(domain): ...`, `feat(api): ...`, `feat(webadmin): ...`, `feat(mobile): ...`).
