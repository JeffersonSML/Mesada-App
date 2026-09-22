# Infra — Mesada App

Infraestrutura como código, schema/migrações do PostgreSQL e scripts de
provisionamento.

Status: a ser criado na Etapa 2 do roadmap (ver [README raiz](../README.md)).

## Conteúdo planejado

```
infra/
  db/
    migrations/     Migrações versionadas do schema (entidades de docs/especificacao.md)
    rls/            Políticas de Row Level Security por familia_id
    seed/           Dados de seed (categorias/tarefas padrão do sistema)
  deploy/           Scripts/manifests de deploy do backend
```

## Isolamento multi-tenant

O isolamento por `familia_id` é reforçado em duas camadas:

1. Filtro explícito por `familia_id` na camada de aplicação (backend)
2. Políticas de Row Level Security no PostgreSQL, como defesa em profundidade
