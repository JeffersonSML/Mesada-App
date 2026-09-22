# Banco de dados — Mesada App

Schema PostgreSQL do sistema, modelado a partir de
[`docs/especificacao.md`](../../docs/especificacao.md#modelo-de-dados--entidades-principais),
com isolamento multi-tenant por `familia_id` reforçado via Row Level Security.

## Provisionamento local

Requer PostgreSQL 16+ e `psql` disponíveis. Duas formas de ter um servidor:

**Opção A — Docker** (recomendado em máquinas sem PostgreSQL instalado):
```bash
cd infra/db
cp .env.example .env   # ajuste as senhas
docker compose up -d
```

**Opção B — servidor local já instalado** (como neste ambiente de
desenvolvimento, que já tem PostgreSQL 16 rodando na porta 5432).

Em ambos os casos, depois de o servidor estar de pé:
```bash
cd infra/db
set -a && source .env && set +a
./scripts/provisionar.sh
```

O script `provisionar.sh` é idempotente: cria o database `mesada` se não
existir, aplica todas as migrações em `migrations/` em ordem numérica, define
a senha das roles de aplicação a partir do ambiente, e roda os seeds em
`seed/`.

## Validando o isolamento multi-tenant

```bash
./scripts/testar_isolamento_rls.sh
```

Cria duas famílias de teste, conecta como a role `mesada_app` escopada a uma
delas e prova que: (1) só enxerga os próprios dados, (2) ainda enxerga as
categorias padrão do sistema, e (3) um `INSERT` cross-tenant é rejeitado pela
política `WITH CHECK`. Remove os dados de teste ao final. Este script deve
continuar passando após qualquer alteração de schema ou de política de RLS.

## Modelo de isolamento (RLS)

Duas roles de banco, nunca a superuser em runtime:

| Role | Uso | RLS |
|---|---|---|
| `mesada_app` | Backend, operações do dia a dia de uma família | Sujeita a RLS por `familia_id` em toda tabela tenant-scoped |
| `mesada_admin` | Módulo Administrador (cross-tenant por natureza) | `BYPASSRLS` — enxerga todas as famílias |

Contrato que o backend precisa cumprir ao conectar como `mesada_app`: no
início de cada transação, executar

```sql
SELECT set_config('app.current_familia_id', '<uuid-da-família-do-usuário-autenticado>', true);
```

(o terceiro parâmetro `true` = escopo `LOCAL` à transação — nunca `SET`
global de sessão, já que conexões são reaproveitadas por um pool entre
requisições de famílias diferentes). Sem essa variável definida, as políticas
retornam zero linhas tenant-scoped — **a falha é fechada, não aberta**.

A criação de uma nova família (fluxo de signup) e todas as operações do
módulo Administrador não têm um `familia_id` de contexto — usam a role
`mesada_admin`, nunca `mesada_app`.

`categorias` é a única tabela híbrida: linhas com `familia_id IS NULL` são os
defaults do sistema (visíveis a todas as famílias em `SELECT`, mas só
graváveis por `mesada_admin`); linhas com `familia_id` preenchido são
customizações de uma família específica.

## Estrutura

```
infra/db/
  migrations/     Schema, em ordem numérica — nunca editar uma migração já
                   aplicada em algum ambiente; crie a próxima.
  seed/           Dados de referência idempotentes (planos, categorias padrão).
  scripts/
    provisionar.sh            Cria DB + roles, aplica migrações e seeds.
    testar_isolamento_rls.sh  Prova de isolamento multi-tenant com dados reais.
  docker-compose.yml   Servidor PostgreSQL local para desenvolvimento/CI.
  .env.example
```

## Convenções para novas migrações

- Um arquivo por mudança de schema, numerado sequencialmente
  (`021_algo.sql`), sempre envolvido em `BEGIN; ... COMMIT;`.
- Toda tabela tenant-scoped carrega `familia_id uuid NOT NULL REFERENCES familias(id)`
  e ganha, na mesma migração ou em `019`-equivalente futura, `ENABLE ROW LEVEL SECURITY`
  + política de isolamento — nunca criar tabela tenant-scoped sem RLS.
- Prefira `numeric(10,2)` para valores monetários (nunca `float`/`double`).
- Chaves primárias são sempre `uuid DEFAULT gen_random_uuid()` — nunca
  `serial`/`bigserial` (evita vazar contagem de linhas entre tenants e
  facilita geração de IDs no cliente offline do app mobile).
- Depois de qualquer mudança que toque RLS ou grants, rode
  `scripts/testar_isolamento_rls.sh` antes de commitar.
