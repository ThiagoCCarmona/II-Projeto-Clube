# 📦 Kit de Inicialização — Projeto Clubes (Execução 100% Local)

Esta pasta contém tudo o que você precisa para **recriar o Projeto Clubes do zero em outro computador ou repositório**, utilizando desenvolvimento assistido por IA com boas práticas de engenharia de software e execução **100% local (sem Docker)**.

---

## 📁 Conteúdo da Pasta

| Arquivo | Para que serve |
| :--- | :--- |
| **[PROMPT.md](./PROMPT.md)** | Prompt mestre completo para colar no seu agente de IA ao iniciar o novo projeto. |
| **[GEMINI.md](./GEMINI.md)** | Arquivo de regras e contexto persistente para o agente (padrões de código, arquitetura, comandos e restrições). |
| **[ARCHITECTURE.md](./ARCHITECTURE.md)** | Especificação técnica completa: Clean Architecture, entidades, regras de negócio, APIs, Blazor e MAUI Offline-First. |
| **[MEMORY.md](./MEMORY.md)** | Memória de lições aprendidas e armadilhas já resolvidas (portas, HTTPS local, bootstrap do WinUI 3 no MAUI, Shell DI, Android). |
| **[AI_WORKFLOW_BEST_PRACTICES.md](./AI_WORKFLOW_BEST_PRACTICES.md)** | Guia prático de boas práticas para conduzir o desenvolvimento com IA passo a passo (Specs, TDD, Checklists). |

---

## 🚀 Como Usar em Outro Lugar (Passo a Passo)

1. Crie uma pasta vazia para o novo projeto (ex.: `mkdir ProjetoClube && cd ProjetoClube`).
2. Copie os arquivos `GEMINI.md`, `ARCHITECTURE.md` e `MEMORY.md` para a **raiz** do novo repositório.
3. Abra seu agente de IA (Antigravity, Gemini CLI, Cursor, etc.) nessa pasta.
4. Cole o conteúdo de **[PROMPT.md](./PROMPT.md)** como primeira mensagem.
5. Ao final de cada fase sugerida no guia, peça para o agente atualizar o `MEMORY.md` e rodar `dotnet test`.
