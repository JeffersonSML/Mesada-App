# ADR 0003: Autenticação, motor de cálculo e integração com RLS

- Status: aceito
- Data: 2026-09-22

## Contexto

ADR 0002 deixou como requisito de entrada da Etapa 3: "o backend .NET
precisará de um mecanismo que, a cada requisição autenticada, resolva a
família do usuário logado e execute o `set_config` antes de qualquer
query". Esta etapa cria o backend .NET (solução em 4 camadas + 2 projetos de
teste) e fecha esse requisito de ponta a ponta.

## Decisões

### 1. Duas DbContext, um único modelo (`MesadaDbContextBase`)

`AppDbContext` (role `mesada_app`) e `AdminDbContext` (role `mesada_admin`)
compartilham o mesmo mapeamento de entidades — só a connection/role muda.
Evita duplicar 15 configurações de entidade para dois contextos que
descrevem o mesmo schema físico.

### 2. Login e resgate de convite rodam sobre `AdminDbContext`, não `AppDbContext`

Login por e-mail e resgate de convite por código são, por definição,
buscas **sem** `familia_id` de contexto ainda estabelecido — é exatamente
isso que a operação está tentando descobrir. Rodar essas buscas sob
`mesada_app` (RLS ativo, sem `app.current_familia_id` definido) retornaria
zero linhas sempre, por design (falha fechada). A solução é usar
`mesada_admin` (`BYPASSRLS`) para esses dois fluxos — seguro porque tanto
`email` (usuarios_master) quanto `codigo` (convites_acesso) são `UNIQUE`
globalmente: a busca não pode vazar dado de outra família, só localiza o
próprio tenant do usuário. Todo endpoint autenticado subsequente usa
`AppDbContext`.

### 3. `TenantConnectionInterceptor` + `Pooling=false` na connection string do `mesada_app`

O interceptor roda `SELECT set_config('app.current_familia_id', '<id>', false)`
ao abrir a conexão física do `AppDbContext`, lendo o claim `familia_id` do
JWT via `ITenantContextAccessor` (implementado em `Mesada.Api`, consumido
por `Mesada.Infrastructure` sem essa depender de ASP.NET Core).

**Simplificação intencional:** o terceiro parâmetro do `set_config` é
`false` (escopo de sessão), não `true`/`LOCAL` (escopo de transação). O
correto para um pool de conexões compartilhado seria `LOCAL` dentro de uma
transação por requisição. Para não vazar o contexto de uma família para
outra requisição que reaproveite a mesma conexão física, a connection
string do `mesada_app` é registrada com `Pooling=false`: cada `AppDbContext`
abre uma conexão própria, então não há reuso capaz de vazar estado entre
requisições. Custo aceito: um handshake TCP+auth novo por requisição.
Revisitar quando performance virar requisito (Etapa 6/7) — as opções são
`SET LOCAL` dentro de uma transação por requisição, ou um pooler ciente de
tenant (ex.: PgBouncer em modo transação).

### 4. Configuração de `IConfiguration` lida sempre via DI, nunca capturada eager em `Program.cs`

`AddMesadaInfrastructure` resolve as `NpgsqlDataSource` via
`AddKeyedSingleton` com fábrica lazy (só constrói no primeiro uso real), e a
configuração do JWT Bearer usa `AddOptions<JwtBearerOptions>().Configure<IOptions<JwtOptions>>(...)`
em vez de ler `builder.Configuration` diretamente no topo do `Program.cs`.
Motivo: `WebApplicationFactory` (testes de integração) sobrescreve
`ConnectionStrings`/`Jwt` via `ConfigureAppConfiguration`, mas essa
sobrescrita só é aplicada depois que o código de nível superior do
`Program.cs` já executou — qualquer leitura eager de configuração ali
captura os valores de *antes* do override do teste e quebra a suíte com
"Host can't be null". Resolver tudo lazily via o container resolve isso e é,
adicionalmente, a prática correta de qualquer forma.

### 5. Motor de cálculo em `Mesada.Domain`, sem dependência nenhuma

`MotorCalculoMesada` é estático, puro, sem I/O — recebe primitivos e enums,
devolve `decimal`/records. Interpretação de uma ambiguidade da spec, tornada
explícita em código e aqui: "Mesada_final = Mesada_base + Σbônus − Σmultas"
não menciona explicitamente o saldo devedor anterior na mesma fórmula, mas o
schema (`ciclos_mesada.saldo_devedor_anterior`) só faz sentido se esse saldo
for deduzido do resultado do ciclo atual antes de decidir se zera ou não.
Implementado como: `valor_final = max(0, bruto_ciclo − saldo_devedor_anterior)`,
com o resto virando `saldo_devedor_resultante`. Também explícito: apenas
tarefas de natureza **Bônus**, quando cumpridas, somam valor extra;
Obrigatórias cumpridas não geram bônus (só evitam a multa).

## Consequências

- Qualquer novo endpoint tenant-scoped usa `AppDbContext` e não precisa
  filtrar por `familia_id` em C# — a política de RLS já faz isso. Filtrar
  manualmente também por `familia_id` é redundante, não incorreto, mas o
  padrão do projeto (ver `FilhosDaFamiliaQuery`) é confiar no RLS.
- Qualquer novo fluxo "pré-tenant" (ex.: signup de nova família) deve seguir
  o mesmo padrão do login: repositório sobre `AdminDbContext`, nunca
  `AppDbContext`.
- `TenantConnectionInterceptor` e a decisão de `Pooling=false` precisam ser
  revisitados junto com qualquer trabalho de performance/carga do backend.
