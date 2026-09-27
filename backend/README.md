# Backend — Mesada App

API RESTful multi-tenant em .NET 8 C#. Requer o .NET 8 SDK e um PostgreSQL
provisionado via [`infra/db`](../infra/db) (schema, roles e RLS).

## Estrutura

```
backend/
  Mesada.sln
  src/
    Mesada.Domain/          Entidades, enums e o motor de cálculo (puro, sem I/O)
    Mesada.Application/     Casos de uso, interfaces (abstrações de infra)
    Mesada.Infrastructure/  EF Core, RLS/tenant context, JWT, hash de senha
    Mesada.Api/             Controllers, Program.cs, appsettings
  tests/
    Mesada.Domain.Tests/       Testes unitários do motor de cálculo
    Mesada.IntegrationTests/   Sobe a Api real contra o Postgres real (WebApplicationFactory)
```

Dependência estritamente em uma direção: `Api → Infrastructure/Application → Domain`.
`Domain` não referencia nenhum pacote externo (nem EF Core) — o motor de
cálculo em `Mesada.Domain/Calculo/MotorCalculoMesada.cs` é 100% testável sem
banco.

## Rodando localmente

1. Provisione o banco (uma vez): siga [`infra/db/README.md`](../infra/db/README.md).
2. Exporte as connection strings e a chave do JWT como variáveis de ambiente
   (nunca commitar segredos em `appsettings.json` — os campos ficam vazios
   propositalmente):

   ```bash
   export ConnectionStrings__MesadaApp="Host=127.0.0.1;Port=5432;Database=mesada;Username=mesada_app;Password=<senha>;Pooling=false"
   export ConnectionStrings__MesadaAdmin="Host=127.0.0.1;Port=5432;Database=mesada;Username=mesada_admin;Password=<senha>"
   export Jwt__Chave="<pelo menos 32 bytes aleatórios>"
   ```

   `Pooling=false` na connection string do `mesada_app` é proposital — ver
   `Mesada.Infrastructure/Tenancy/TenantConnectionInterceptor.cs` e
   [`docs/adr/0003-autenticacao-e-tenant-context.md`](../docs/adr/0003-autenticacao-e-tenant-context.md).

3. `dotnet run --project src/Mesada.Api` — API sobe com Swagger em `/swagger`
   no ambiente Development.

## Testes

```bash
dotnet test                              # unitários + integração (precisa do Postgres de pé)
dotnet test tests/Mesada.Domain.Tests    # só o motor de cálculo, sem banco
```

95 testes no total (27 unitários + 68 de integração). Os testes de integração usam o mesmo Postgres
provisionado por `infra/db` — criam família(s) de teste com o prefixo
`[teste-e2e]`, batem na Api real via `WebApplicationFactory<Program>` e
removem os dados ao final (`IAsyncLifetime.DisposeAsync`). Os testes dos
adapters de Pagar.me/Resend/S3 sobem um servidor HTTP local que imita a API
real de cada provedor — nenhum tem chave de sandbox neste ambiente (ver
[`docs/adr/0006-*.md`](../docs/adr/0006-infra-servicos-externos.md)).

## O que já está implementado (Etapa 3)

- **Motor de cálculo** (`Mesada.Domain.Calculo.MotorCalculoMesada`): valor por
  tarefa (Valor Direto / Pontos), multa, sugestão automática de valor/pontos,
  resumo de ciclo e fechamento de ciclo — fórmulas de
  [`docs/especificacao.md#lógica-de-cálculo`](../docs/especificacao.md#lógica-de-cálculo),
  27 testes unitários cobrindo os casos e limites da spec.
- **Autenticação**: login de Master (e-mail/senha, JWT) e resgate de convite
  pelo app do filho (código → vincula dispositivo → JWT). Hash de senha via
  BCrypt; claims do JWT documentadas em `Mesada.Application.Auth.MesadaClaimTypes`.
- **Integração RLS ↔ Api**: `TenantConnectionInterceptor` popula
  `app.current_familia_id` a cada conexão do `AppDbContext`, a partir do
  claim `familia_id` do JWT — provado por teste de integração que bate no
  endpoint `GET /api/filhos` autenticado como Master de uma família e
  confirma que filhos de outra família nunca aparecem.
- **Persistência**: `AppDbContext` (role `mesada_app`, sujeito a RLS) e
  `AdminDbContext` (role `mesada_admin`, `BYPASSRLS`, usado pelo login e pelo
  resgate de convite — que por definição rodam antes de existir um tenant
  identificado). Mapeamento completo das entidades do schema de
  `infra/db/migrations`, incluindo os enums nativos do Postgres via
  `NpgsqlDataSourceBuilder.MapEnum`.

Decisões e trade-offs desta etapa registrados em
[`docs/adr/0003-autenticacao-e-tenant-context.md`](../docs/adr/0003-autenticacao-e-tenant-context.md).

## O que já está implementado (Etapa 6)

- **Pagamento**: `IPaymentProvider` implementado sobre o SDK oficial
  Stone/Pagar.me (`Mesada.Infrastructure.Payments.PagarMePaymentProvider`).
- **E-mail**: `IEmailSender` sobre o SDK oficial Resend.
- **Push**: `IPushNotificationSender` sobre o SDK oficial FirebaseAdmin (FCM).
- **Storage de evidências**: `IEvidenceStorageService` sobre o AWS SDK
  (`AWSSDK.S3`), apontável para qualquer S3-compatível via `S3:ServiceUrl`
  (MinIO em desenvolvimento).
- **Deploy**: `Dockerfile` multi-stage nesta pasta; stack de referência
  (Postgres + MinIO + API) em [`infra/deploy`](../infra/deploy).

Nenhum dos quatro provedores tem credencial real configurada neste
ambiente — cada um foi validado contra um servidor HTTP local que imita a
API real, não contra o provedor de verdade. Ver
[`docs/adr/0006-infra-servicos-externos.md`](../docs/adr/0006-infra-servicos-externos.md)
para o que foi (e não foi) provado.

## Endpoints (Etapa 7 — orquestração HTTP dos casos de uso)

Toda a superfície abaixo é tenant-scoped (`AppDbContext`/`mesada_app`/RLS),
exceto onde indicado como pré-tenant (`AdminDbContext`/`mesada_admin`).

**Convenções cross-cutting** (descobertas rodando um frontend de verdade
contra a API real, não só pelos testes de integração):
- Enums sempre como **string** no JSON (`JsonStringEnumConverter` em
  `Program.cs`), nunca número — é o contrato documentado aqui e o que os
  clientes reais (Web, mobile) enviam/esperam.
- **CORS**: origens permitidas configuradas em `Cors:OrigensPermitidas`
  (`appsettings.json` ou variável de ambiente `Cors__OrigensPermitidas__0`,
  `__1`, ...) — por padrão inclui o preview do Lovable e
  `http://localhost:5173` (dev local da Web). Sem isso, qualquer frontend
  em outra origem tem toda chamada bloqueada pelo navegador antes de chegar
  à API.

- **Auth** (pré-tenant): `POST /api/auth/master/login`,
  `POST /api/auth/convites/{codigo}/resgatar` (filho/Comum),
  `POST /api/auth/convites/{codigo}/resgatar-master` (segundo responsável).
- **Signup** (pré-tenant): `POST /api/familias` — cria a Família e o primeiro
  Master (sempre financeiro), já devolvendo um token.
- **Minha Família**: `GET/PUT /api/familias` (nome, ciclo de fechamento padrão).
- **Filhos**: `GET/POST/PUT/DELETE /api/filhos` (remoção lógica).
- **Segundo(s) Responsável(is)**: `GET/DELETE /api/masters` — cadastro só
  via Convite (`papelAlvo: "Master"`); bloqueia autodesativação e desativar
  o único Master financeiro ativo.
- **Convites**: `GET/POST/DELETE(revogar) /api/convites` — aceita
  `papelAlvo` Comum (vincula um filho) ou Master (convida um segundo
  responsável, resgatado em `/resgatar-master`).
- **Categorias**: `GET/POST/PUT/DELETE /api/categorias` — modelo híbrido
  (defaults do sistema + customizadas da família); remoção é lógica.
- **Tarefas**: `GET/POST/PUT/DELETE /api/tarefas`,
  `POST/DELETE /api/tarefas/{id}/aderencias`,
  `GET /api/tarefas/sugestao?usuarioComumId=&modoCalculo=`.
- **Execuções**: `POST /api/execucoes` (Comum marca conclusão),
  `POST /api/execucoes/{id}/aprovar`, `POST /api/execucoes/{id}/rejeitar`
  (Master), `GET /api/execucoes/pendentes` (Master) — resposta inclui
  `nomeTarefa`/`nomeFilho` já resolvidos (evita o Master ver só o
  `tarefaUsuarioId` cru na tela de aprovação), `GET /api/tarefas/minhas`
  (Comum).
- **Ciclos**: `POST /api/ciclos/fechar`, `GET /api/ciclos/historico`,
  `GET /api/ciclos/atual` (prévia sem persistir, desde o fim do último
  ciclo fechado).
- **Notificações**: `GET/POST/PUT/DELETE /api/notificacoes/destinatarios` —
  e-mails/telefones extras que recebem os alertas da família, além do
  cadastro principal do Master/Comum (não confundir com credenciais dos
  provedores Resend/FCM, que são segredo de infraestrutura via ambiente).

## Módulo Administrador (Etapa 8 — painel interno, grupos de acesso)

Fora de qualquer contexto de família — login, tabelas e RLS totalmente
segregados do restante da API (ver
[`docs/adr/0007-modulo-administrador-mesma-app.md`](../docs/adr/0007-modulo-administrador-mesma-app.md)).
O grupo `"Owner"` (`sistema: true`) tem acesso total e não pode ser editado
nem removido; qualquer outro grupo (ex.: `"Tecnologia"`) é criado
livremente pelo Owner.

- **Auth**: `POST /api/admin/auth/login`, `POST /api/admin/auth/trocar-senha`,
  `PUT /api/admin/auth/email`. Um Administrador recém-criado ou com senha
  resetada nasce com `deveTrocarSenha: true`.
- **Administradores**: `GET /api/admin/administradores` (qualquer
  Administrador), `POST/PUT/DELETE /api/admin/administradores` (só grupo
  Owner) — convite gera uma senha temporária devolvida uma única vez.
- **Grupos**: `GET /api/admin/grupos` (qualquer Administrador),
  `POST/PUT/DELETE /api/admin/grupos` (só grupo Owner).
- Bootstrap do primeiro Administrador (Owner):
  [`infra/db/scripts/criar_administrador_owner.sh`](../infra/db/scripts/criar_administrador_owner.sh)
  — nunca commita senha em texto claro, gera o hash via `pgcrypto` no
  próprio Postgres.

## Ainda não implementado (próximos passos do backend)

- Demais telas do Módulo Administrador previstas na spec — Famílias,
  Assinaturas, Suporte, Métricas, Categorias Padrão — e endpoints de
  Planos/Assinatura da própria família.
- Cálculo automático das datas de um ciclo a partir de
  `CicloPeriodicidade` (hoje o Master informa `dataInicio`/`dataFim`
  explicitamente ao fechar) — depende de um agendador de fechamento.
- Validação com credenciais reais dos quatro serviços externos (Pagar.me,
  Resend, Firebase, S3/MinIO) — depende de contas que ainda não existem.
