# Backend — Mesada App

API RESTful multi-tenant em .NET C#.

Status: a ser criado na Etapa 3 do roadmap (ver [README raiz](../README.md)).

## Responsabilidades

- Autenticação OAuth2/JWT (Masters e Comuns via convite)
- Motor de cálculo de mesada (modos Valor Direto e Pontos, fechamento de ciclo,
  sugestão automática de valor — ver [`docs/especificacao.md`](../docs/especificacao.md#lógica-de-cálculo))
- Isolamento multi-tenant por `familia_id`, reforçado por Row Level Security
  no PostgreSQL
- Fluxo de aprovação de tarefas e notificações (FCM/e-mail)
- Integração com `IPaymentProvider` (Asaas/Stripe) e storage de evidências
  compatível com S3
- Módulo Administrador (famílias, assinaturas, suporte, métricas)

## Estrutura planejada

```
backend/
  src/
    Mesada.Api/            Camada de apresentação (controllers, DTOs, auth)
    Mesada.Application/    Casos de uso, motor de cálculo
    Mesada.Domain/         Entidades e regras de domínio
    Mesada.Infrastructure/ EF Core, RLS, IPaymentProvider, storage, notificações
  tests/
    Mesada.Application.Tests/
    Mesada.Domain.Tests/
  Mesada.sln
```
