# Documentação do Projeto — App de Controle Financeiro Pessoal

**Autor:** Claudio Nazareth Lopes
**Versão:** 1.1 — atualizado com decisões de autenticação, hospedagem, banco de dados e nova solicitação de sugestão de limites
**Data:** 05/09/2026

---

## 1. Visão geral

Aplicação pessoal para registrar gastos e receitas do dia a dia ("gastei, aí lanço") e gerar automaticamente um relatório mensal consolidado. O objetivo não é competir com apps de finanças prontos do mercado (Mobills, Organizze etc.) — é ter uma ferramenta sob seu controle, com o fluxo de lançamento que você quer, sem assinatura e sem depender de terceiros.

Ponto de partida assumido: uso individual (você), sem necessidade de multiusuário na v1.

---

## 2. Problema a resolver

Hoje não existe um registro estruturado de gastos: cada saída de dinheiro precisa ser lançada manualmente em algum lugar, e no fim do mês não há visão consolidada de quanto entrou, quanto saiu, em quê, e se ficou dentro do esperado. O app resolve isso com três peças: (1) lançamento rápido, (2) categorização, (3) relatório mensal automático.

---

## 3. Escopo funcional

### 3.1 MVP (Fase 1)

- Lançar gasto: valor, data, descrição, categoria, forma de pagamento (dinheiro, débito, crédito, pix).
- Lançar receita: valor, data, descrição, categoria/fonte (salário, freelance, outro).
- Categorias: conjunto padrão pré-cadastrado (alimentação, transporte, moradia, lazer, saúde, salário, outros) + criação de categorias customizadas.
- Relatório mensal: total de receitas, total de gastos, saldo do mês, gasto por categoria (valor e %), comparação com o mês anterior.

### 3.2 Fase 2

- Orçamento/meta mensal: definir um limite geral e/ou por categoria; indicador visual de quanto já foi consumido do limite.
- Alerta ao ultrapassar o orçamento de uma categoria ou o limite geral.

### 3.3 Sugestão de limites de orçamento (nova solicitação — vale abrir antes de aceitar no escopo)

Você pediu para incluir uma IA que analise suas finanças e ajude a definir os limites de orçamento. Antes de comprar essa ideia como está, vale separar duas coisas que estão misturadas sob o rótulo "IA":

- **O que você provavelmente precisa:** olhar o histórico de gastos e sugerir um limite razoável por categoria. Isso é um problema estatístico simples — média dos últimos 3 meses, ou média + margem de segurança — que não exige nenhuma chamada de IA, não tem custo por requisição e não manda seus dados financeiros pra uma API de terceiros.
- **O que só uma IA generativa realmente entrega:** insight em linguagem natural sobre padrões ("seu gasto com lazer subiu 40% este mês", "você está gastando mais do que ganha há 2 meses seguidos"). Isso vai além de sugerir um número — e só se justifica se a versão estatística não for suficiente na prática.

Isso é, literalmente, o risco de scope creep que a seção 10 já apontava se materializando. Recomendação: implementar a sugestão estatística na Fase 3 (abaixo) e só evoluir para uma chamada de LLM na Fase 4 se, no uso real, a sugestão estatística deixar a desejar.

### 3.4 Fase 3 (opcional, avaliar depois de usar o MVP por um tempo)

- PWA instalável com suporte a lançamento offline (fila local + sincronização quando voltar a conexão).
- Exportação do relatório mensal em PDF/Excel.
- Gráficos de evolução (últimos 6/12 meses).
- Sugestão estatística de limite por categoria (média dos últimos meses + margem).

### 3.5 Fase 4 (opcional, só se a Fase 3 não for suficiente)

- Análise via LLM (Claude API, Azure OpenAI etc.) para gerar insights em linguagem natural sobre os gastos, além de sugerir limites.
- Implica: custo por chamada, prompt/integração a manter, e decisão consciente de mandar dados financeiros agregados para uma API externa.

### 3.6 Fora de escopo (v1) — decisão deliberada

Para não virar um projeto infinito, ficam de fora por padrão, mesmo sendo "features naturais" de app financeiro:

- Integração bancária automática (Open Finance) — complexidade regulatória e técnica alta para o ganho que traz num app de uso pessoal.
- Múltiplas contas bancárias/cartões com conciliação de fatura.
- Multiusuário / compartilhamento familiar.
- Controle de investimentos.

Se alguma dessas fizer falta na prática, ela entra como uma fase nova — não como retrabalho do que já foi construído, se o modelo de dados abaixo for respeitado.

---

## 4. Stack tecnológica e por que essa escolha

Seu currículo mostra 5+ anos como desenvolvedor .NET sênior (C#, .NET Core, ASP.NET MVC, Web API, SQL Server/T-SQL, Azure DevOps), com Clean Code e Design Patterns como prática. Não há experiência mobile nativa (Android/Kotlin, iOS/Swift) registrada. A escolha de stack parte disso — não da opção mais "moderna" do mercado, mas da que você já sabe operar sem curva de aprendizado extra num projeto pessoal.

| Camada | Escolha | Justificativa |
|---|---|---|
| Backend | ASP.NET Core Web API (C#) | Você já usa isso profissionalmente todo dia |
| ORM/Dados | Entity Framework Core | Reduz SQL boilerplate; você já domina T-SQL para os casos em que o EF não é suficiente |
| Banco de dados | **SQLite** (decidido) | Volume de dados de um controle pessoal é baixo — SQLite dispensa custo e manutenção de um servidor de banco. Fica no storage persistente do próprio App Service (`/home`), que sobrevive a reinícios dentro do mesmo App Service Plan |
| Frontend | Blazor WebAssembly, empacotado como PWA | Reaproveita 100% C#, evita aprender um framework JS só para isso; PWA permite "instalar" no celular e abrir como app |
| Autenticação | ASP.NET Core Identity (email/senha) + WebAuthn/Passkey (Face ID no iPhone) | Identity resolve hash de senha, lockout etc. sem reinventar; WebAuthn é suportado pela Apple em PWA instalada na tela inicial — mas fica preso ao dispositivo que registrou a credencial, então email/senha continua sendo o método base, não substituível |
| Hospedagem | **Azure App Service, tier F1 (gratuito)** | Free permanente e você já tem familiaridade com Azure. Trade-off: sem "Always On" — a aplicação "dorme" após inatividade e tem cold start (alguns segundos, possivelmente ~30s) na primeira requisição seguinte; limite de 60 min de CPU/dia, que sobra para uso pessoal esporádico |

### Alternativas consideradas e descartadas

- **.NET MAUI (app nativo Android/iOS):** foi a primeira ideia óbvia para "lançar gasto no celular", mas seu currículo não mostra nenhuma experiência mobile — MAUI teria curva de aprendizado real (ciclo de vida de app, XAML, empacotamento para loja) para um projeto que é ferramenta pessoal, não produto. Reavaliar só se o PWA se mostrar limitado demais no uso real.
- **Blazor Server em vez de WebAssembly:** mais simples de hospedar (um único processo, sem separar API/frontend), mas depende de conexão constante via SignalR — ruim justamente para o caso de uso principal (lançar um gasto na rua, com sinal ruim). Por isso a escolha caiu em WASM + API, mesmo sendo uma peça a mais de infraestrutura.
- **Planilha (Sheets/Excel):** resolveria o relatório mensal com esforço quase zero, mas você pediu documentação técnica de projeto, não uma planilha — então assumo que o objetivo é construir o app mesmo, com a planilha descartada como opção.

**Risco assumido:** separar API e frontend WASM é mais peça de infraestrutura do que um monólito único, para um sistema de um usuário só. Isso é uma troca consciente pela resiliência de lançamento fora de casa — se isso não for prioridade real, um Blazor Server monolítico é mais simples de manter e deveria ser reconsiderado antes de começar a codar.

**Ajuste de deploy para caber no tier gratuito:** em vez de publicar API e Blazor WASM como dois recursos separados no Azure, use o template "ASP.NET Core hosted" do Blazor WebAssembly — a API e os arquivos estáticos do WASM saem de um único projeto/deploy, ocupando um único App Service. Mantém a separação lógica de camadas da seção 5, só simplifica a topologia de hospedagem.

---

## 5. Arquitetura

```
[Blazor WASM PWA] --HTTP/JSON--> [ASP.NET Core Web API] --EF Core--> [SQLite/SQL Server]
```

Organização em camadas (Clean Architecture simplificada):

- **Domain:** entidades (Lancamento, Categoria, OrcamentoMensal) e regras de negócio puras (ex.: "orçamento não pode ser negativo").
- **Application:** casos de uso (RegistrarGasto, RegistrarReceita, GerarRelatorioMensal, DefinirOrcamento), interfaces de repositório.
- **Infrastructure:** implementação dos repositórios com EF Core, acesso a dados.
- **API:** controllers (ou Minimal APIs) expondo os casos de uso via REST.
- **Web:** Blazor WASM consumindo a API.

Isso mantém a lógica de negócio testável e independente do EF Core ou da API — coerente com "Clean Code e Design Patterns" já citados no seu currículo como prática.

---

## 6. Modelo de dados (entidades principais)

**Categoria**
- Id
- Nome
- Tipo (Gasto | Receita)
- Cor/Ícone (opcional, para UI)
- PadraoDoSistema (bool — categorias pré-cadastradas não podem ser excluídas, só desativadas)

**Lancamento**
- Id
- Tipo (Gasto | Receita)
- Valor
- Data
- Descricao
- CategoriaId (FK)
- FormaPagamento (Dinheiro | Debito | Credito | Pix) — aplicável a Gasto
- Observacao (opcional)

**OrcamentoMensal**
- Id
- CategoriaId (FK, nullable — nulo representa o orçamento geral do mês)
- MesReferencia (ano-mês)
- ValorLimite

O relatório mensal é **calculado on-the-fly** a partir de `Lancamento` — não precisa de uma tabela própria. Isso evita inconsistência entre relatório e dados reais.

Campo `UsuarioId` fica de fora de `Categoria`, `Lancamento` e `OrcamentoMensal` na v1 (uso individual) — só entraria como FK se multiusuário virar necessidade real.

Com a autenticação decidida, entram duas tabelas padrão do ASP.NET Core Identity (usuário, com email/senha) e uma tabela de credenciais WebAuthn vinculada a esse usuário (para o login via Face ID). São tabelas geradas pelo próprio framework, não modelagem extra sua.

---

## 7. Fluxos principais

1. **Lançar gasto rápido:** abrir app → botão "novo gasto" → valor + categoria (campos obrigatórios mínimos) → data e descrição pré-preenchidas com hoje/vazio → salvar. Meta: menos de 15 segundos do abrir ao salvar.
2. **Lançar receita:** mesmo fluxo, com tipo = Receita.
3. **Configurar orçamento mensal:** tela de configuração → definir limite geral e/ou por categoria para o mês corrente.
4. **Consultar relatório mensal:** tela de relatório → seleciona mês (padrão: mês atual) → vê totais, saldo, gasto por categoria, comparação com mês anterior.
5. **Alerta de orçamento estourado:** ao lançar um gasto que ultrapasse o limite da categoria ou o limite geral do mês, exibir aviso (não bloquear o lançamento).

---

## 8. Requisitos não funcionais

- **Velocidade de lançamento:** o fluxo de "novo gasto" é o mais usado — deve ser o mais curto possível (poucos campos obrigatórios, sem telas intermediárias).
- **Uso mobile:** PWA instalável na tela inicial do celular, layout responsivo mobile-first (é onde o lançamento vai acontecer na prática).
- **Autenticação:** email/senha (ASP.NET Core Identity) como método base + Face ID via WebAuthn/Passkey como conveniência no iPhone — sem RBAC ou multiusuário na v1.
- **Custo de operação:** hospedagem no tier gratuito do Azure App Service (F1) — zero custo mensal, aceitando cold start ocasional como trade-off.

---

## 9. Roadmap

| Fase | Entregável | Critério de pronto |
|---|---|---|
| 1 — MVP | Lançamento de gasto/receita, categorias, relatório mensal básico | Você consegue lançar um gasto e ver o relatório do mês corrente |
| 2 | Orçamento/meta mensal + alertas | Você define um limite e recebe aviso ao estourar |
| 3 (opcional) | PWA offline + sync, exportação, gráficos, sugestão estatística de limite por categoria | Avaliar só depois de usar o MVP por pelo menos 1–2 meses reais |
| 4 (opcional) | Análise via LLM: insights em linguagem natural sobre os gastos | Só entra se a sugestão estatística da Fase 3 não for suficiente na prática |

---

## 10. Riscos e pontos em aberto

- **Scope creep continua sendo o maior risco real aqui** — e a ideia de IA para analisar as finanças (seção 3.3) é esse risco se materializando de forma concreta. Mitigado adiando a versão com LLM para a Fase 4, condicionada a a versão estatística da Fase 3 não ser suficiente.
- **WebAuthn/Face ID depende do dispositivo:** a credencial biométrica fica presa ao iPhone que a registrou — trocar de aparelho ou reinstalar o PWA derruba o acesso via Face ID até reconfigurar. A Apple documenta suporte a WebAuthn em PWA instalada na tela inicial, mas o comportamento real no seu iPhone específico não foi validado aqui — testar o fluxo completo (registro + login) antes de depender dele como método principal.
- **Cold start no Azure App Service F1:** sem "Always On", a primeira requisição após um período parado pode demorar alguns segundos a mais. Se isso incomodar no uso real (ex. lançar um gasto rápido na rua), a saída é subir para o tier Basic (baixo custo) — não é motivo para trocar de provedor agora.
- Decisões de autenticação, hospedagem e banco de dados que estavam em aberto na v1.0 foram resolvidas nesta rodada: email/senha + Face ID (WebAuthn), Azure App Service F1 gratuito, SQLite.
