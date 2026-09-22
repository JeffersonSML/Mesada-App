# ADR 0002: Schema inicial em SQL puro + RLS com duas roles

- Status: aceito
- Data: 2026-09-22

## Contexto

O backend .NET (Etapa 3) ainda não existe, mas o schema do PostgreSQL e o
mecanismo de isolamento multi-tenant precisam ser definidos e validados antes
dele, já que o motor de cálculo e a autenticação vão depender diretamente
dessas decisões.

## Decisões

1. **Migrações em SQL puro, numeradas sequencialmente** (`infra/db/migrations`),
   em vez de esperar pelo EF Core Code-First. Justificativa: o schema é o
   contrato entre backend, web e apps mobile, e precisa existir de forma
   independente de qualquer ORM. Quando o backend .NET for criado (Etapa 3),
   ele pode consumir este schema via Database First (scaffold) ou o time
   pode migrar para EF Core Migrations reaproveitando este DDL como baseline.

2. **Duas roles de banco, nunca superusuário em runtime:**
   - `mesada_app`: usada pelo backend nas operações do dia a dia de uma
     família. Sujeita a Row Level Security por `familia_id`.
   - `mesada_admin`: usada exclusivamente pelo módulo Administrador.
     `BYPASSRLS`, porque o Administrador precisa enxergar todas as famílias
     por natureza (suporte, métricas, gestão de assinaturas).

3. **Contrato de sessão para RLS:** o backend deve executar
   `SELECT set_config('app.current_familia_id', '<uuid>', true)` no início
   de cada transação ao conectar como `mesada_app`. Sem essa variável, as
   políticas retornam zero linhas — falha fechada por padrão.

4. **`categorias` é a única tabela híbrida** (`familia_id` nulo = padrão do
   sistema, visível a todas as famílias; preenchido = customização de uma
   família), refletindo o modelo híbrido descrito na especificação.

5. **Chaves primárias `uuid` (`gen_random_uuid()`), nunca `serial`**, para não
   vazar contagem de linhas entre tenants e para permitir geração de IDs no
   cliente offline do app mobile antes da sincronização.

6. **`familia_id` denormalizado em toda tabela tenant-scoped**, mesmo quando
   alcançável por join (ex.: `execucoes`, `evidencias`, `historico_cobranca`),
   para manter as políticas de RLS simples (comparação direta de coluna, sem
   subconsulta `EXISTS`) e previsíveis em performance.

## Validação

`infra/db/scripts/testar_isolamento_rls.sh` cria duas famílias de teste, roda
consultas e um `INSERT` cross-tenant como `mesada_app` e confirma que o
isolamento é reforçado pelo banco, não apenas pela aplicação. Este script
deve continuar passando após qualquer alteração de schema ou de política de
RLS — é o critério de aceite de qualquer mudança no `infra/db`.

## Consequências

- Toda nova tabela tenant-scoped precisa nascer com `familia_id NOT NULL`,
  RLS habilitado e política de isolamento na mesma migração — não é opcional.
- O backend .NET precisará de um mecanismo (ex.: interceptor do EF Core ou
  middleware) que, a cada requisição autenticada, resolva a família do
  usuário logado e execute o `set_config` antes de qualquer query — este
  ponto fica registrado como requisito de entrada da Etapa 3.
