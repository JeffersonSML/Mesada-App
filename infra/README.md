# Infra — Mesada App

Infraestrutura como código, schema/migrações do PostgreSQL e scripts de
provisionamento.

## Conteúdo

```
infra/
  db/
    migrations/     Schema versionado, em ordem numérica (ver db/README.md)
    seed/           Dados de referência (planos e categorias padrão do sistema)
    scripts/        Provisionamento e validação de isolamento multi-tenant
    docker-compose.yml
  deploy/           Scripts/manifests de deploy do backend (Etapa 6)
```

Documentação completa do schema, do modelo de RLS e de como provisionar um
banco localmente: [`db/README.md`](db/README.md).

## Isolamento multi-tenant

O isolamento por `familia_id` é reforçado em duas camadas:

1. Filtro explícito por `familia_id` na camada de aplicação (backend)
2. Row Level Security no PostgreSQL, como defesa em profundidade — validada
   automaticamente por [`db/scripts/testar_isolamento_rls.sh`](db/scripts/testar_isolamento_rls.sh)
