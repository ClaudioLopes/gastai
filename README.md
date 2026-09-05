# GastAi — Controle Financeiro Pessoal

App pessoal para lançar gastos/receitas do dia a dia e ver um relatório mensal consolidado.
Documentação completa do projeto (visão, escopo, arquitetura, roadmap): [docs/documentacao-controle-financeiro.md](docs/documentacao-controle-financeiro.md).

## Status

**Fase 1 (MVP)** — concluída: lançamento de gasto/receita, categorias (padrão + customizadas) e relatório mensal com comparação ao mês anterior.

## Stack

ASP.NET Core Web API + EF Core + SQLite, servindo um front-end Blazor WebAssembly (PWA) a partir do mesmo processo/deploy.

## Estrutura

```
src/
  GastAi.Domain          entidades e regras de negócio
  GastAi.Contracts       DTOs compartilhados entre API e Web
  GastAi.Application     casos de uso (serviços de aplicação)
  GastAi.Infrastructure  EF Core (DbContext, migrations, repositórios)
  GastAi.Api             Web API (controllers) + hospeda os arquivos do Blazor WASM
  GastAi.Web             Blazor WebAssembly (PWA)
tests/
  GastAi.Domain.Tests       regras de negócio das entidades
  GastAi.Application.Tests  casos de uso, com repositórios mockados (Moq)
  GastAi.Api.Tests          testes de integração ponta a ponta (WebApplicationFactory + SQLite temporário)
```

## Rodando localmente

Pré-requisito: [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/GastAi.Api/GastAi.Api.csproj
```

Abre em `http://localhost:5115`. As migrations do EF Core são aplicadas automaticamente na inicialização e o banco SQLite fica em `src/GastAi.Api/App_Data/gastai.db` (não versionado).

Para criar uma nova migration após alterar o modelo de dados:

```bash
dotnet ef migrations add NomeDaMigration --project src/GastAi.Infrastructure --startup-project src/GastAi.Api -o Persistence/Migrations
```

## Rodando os testes

```bash
dotnet test
```
