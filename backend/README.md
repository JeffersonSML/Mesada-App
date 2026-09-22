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

Os testes de integração usam o mesmo Postgres provisionado por `infra/db` —
criam família(s) de teste com o prefixo `[teste-e2e]`, batem na Api real via
`WebApplicationFactory<Program>` e removem os dados ao final
(`IAsyncLifetime.DisposeAsync`).

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

## Ainda não implementado (próximos passos do backend)

- CRUD de categorias/tarefas, aprovação de execuções, fechamento de ciclo via
  endpoint (o motor de cálculo já existe e está testado; falta a orquestração/HTTP).
- Cadastro de nova família (signup) e criação de convites pelo Master.
- `IPaymentProvider` concreto (Asaas/Stripe), storage S3 de evidências,
  notificações (FCM/e-mail) — Etapa 6.
- Módulo Administrador (endpoints — o `AdminDbContext` já existe e já tem
  `BYPASSRLS`, falta a superfície HTTP).
