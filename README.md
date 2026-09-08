# GastAi — Controle Financeiro Pessoal

App pessoal para lançar gastos/receitas do dia a dia e ver um relatório mensal consolidado.
Documentação completa do projeto (visão, escopo, arquitetura, roadmap): [docs/documentacao-controle-financeiro.md](docs/documentacao-controle-financeiro.md).

## Status

**Fase 1 (MVP)** — concluída: lançamento de gasto/receita, categorias (padrão + customizadas) e relatório mensal com comparação ao mês anterior.

**Fase 2** — concluída: orçamento/meta mensal geral e por categoria, com indicador visual de consumo e alerta (não bloqueante) ao lançar um gasto que ultrapassa o limite.

**Autenticação** — concluída: login com e-mail/senha (ASP.NET Core Identity). App de uso individual — não há registro público; o único usuário (o dono) é criado no primeiro start a partir de `Auth:OwnerEmail`/`Auth:OwnerPassword`. WebAuthn/Passkey (mencionado na documentação como conveniência sobre esse método base) fica para depois.

**Publicado em produção:** https://gastai.fly.dev (Fly.io, plano gratuito).

## Stack

ASP.NET Core Web API + EF Core + SQLite, servindo um front-end Blazor WebAssembly (PWA) a partir do mesmo processo/deploy. Hospedado no Fly.io (Docker + volume persistente para o SQLite), no lugar do Azure App Service originalmente cogitado na documentação.

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

Para logar localmente, defina as credenciais do dono via user-secrets antes do primeiro `dotnet run` (só cria o usuário se ainda não existir nenhum):

```bash
cd src/GastAi.Api
dotnet user-secrets set "Auth:OwnerEmail" "seu-email@exemplo.com"
dotnet user-secrets set "Auth:OwnerPassword" "uma-senha-forte"
```

Para criar uma nova migration após alterar o modelo de dados:

```bash
dotnet ef migrations add NomeDaMigration --project src/GastAi.Infrastructure --startup-project src/GastAi.Api -o Persistence/Migrations
```

## Rodando os testes

```bash
dotnet test
```

## Deploy (Fly.io)

`Dockerfile` + `fly.toml` na raiz do repo. A máquina hiberna quando ociosa (`auto_stop_machines`) e acorda na primeira requisição seguinte — mesmo trade-off de cold start que estava previsto para o Azure App Service F1. O SQLite e as chaves de criptografia do cookie de login ficam num volume persistente (`gastai_data`, montado em `/data`), então sobrevivem a reinícios e redeploys.

```bash
flyctl deploy -a gastai
```

Credenciais de login em produção são secrets do Fly, não ficam no repositório:

```bash
flyctl secrets set -a gastai "Auth__OwnerEmail=..." "Auth__OwnerPassword=..."
```

(Só têm efeito antes do primeiro usuário ser criado — depois disso, trocar a senha exige apagar o usuário direto no banco.)
