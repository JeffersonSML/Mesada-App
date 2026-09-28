# Mesada App

SaaS multi-tenant para controle de mesada de filhos, com tarefas parametrizáveis,
pontuação/valor/percentual de conclusão, fluxo de aprovação, planos de assinatura
e módulo administrador.

Cada família é um tenant isolado (`familia_id`) via Row Level Security no PostgreSQL.

## Especificação

A especificação funcional completa (visão geral, perfis de usuário, lógica de
cálculo, modelo de dados, telas) está versionada em [`docs/especificacao.md`](docs/especificacao.md),
com um adendo por plataforma que tem comportamento específico o bastante
para merecer o próprio documento: [`docs/adendo-web.md`](docs/adendo-web.md)
(Painel do Master + Painel Administrativo) e
[`docs/adendo-mobile.md`](docs/adendo-mobile.md) (Android/iOS).

## Estrutura do repositório

Mono-repo: todos os componentes do produto vivem aqui, versionados juntos, já
que backend/web/mobile compartilham contrato de API, schema de dados e o mesmo
ritmo de evolução enquanto o produto é construído por um único time.

```
/backend          API .NET C# (RESTful, multi-tenant, motor de cálculo de mesada)
/web              Painel do Master + Painel Administrativo — código vive no Lovable; ver web/README.md
/mobile-android   App nativo Kotlin (uso exclusivo do usuário Comum/filho)
/mobile-ios       App nativo Swift (uso exclusivo do usuário Comum/filho)
/infra            Infraestrutura: schema/migrações PostgreSQL, IaC, scripts de deploy
/docs             Especificação funcional + adendos (Web, Mobile), ADRs (decisões de arquitetura)
/.github/workflows CI/CD
```

## Stack técnica

| Camada | Tecnologia |
|---|---|
| Backend | .NET C# — API RESTful multi-tenant |
| Banco de dados | PostgreSQL com Row Level Security |
| Web | React + TypeScript, via [Lovable](https://lovable.dev) (ver [adendo Web](docs/adendo-web.md), [ADR 0004](docs/adr/0004-web-via-lovable.md)) |
| Mobile Android | Kotlin nativo (ver [adendo Mobile](docs/adendo-mobile.md)) |
| Mobile iOS | Swift nativo (ver [adendo Mobile](docs/adendo-mobile.md)) |
| Offline (mobile) | SQLite local + fila de sincronização |
| Autenticação | OAuth2/JWT + biometria no mobile |
| Pagamentos | Stone/Pagar.me, atrás de `IPaymentProvider` (ver [ADR 0006](docs/adr/0006-infra-servicos-externos.md)) |
| Notificações | Firebase Cloud Messaging (push) + SendGrid/Resend (e-mail) |
| Storage de evidências | Compatível com S3 |

## Roadmap de construção

1. Estruturar o repositório no GitHub ✅
2. Provisionar o PostgreSQL e modelar o schema inicial ✅
3. Criar o projeto backend .NET com autenticação e o motor de cálculo ✅
4. Criar o projeto Web (via Lovable) ✅
5. Criar os projetos mobile nativos (Android e iOS) ✅ (não compilados neste ambiente — ver [ADR 0005](docs/adr/0005-mobile-restricoes-de-ambiente.md))
6. Configurar os serviços de infraestrutura (push, e-mail, storage, pagamento) ✅ (implementados, sem credenciais reais neste ambiente — ver [ADR 0006](docs/adr/0006-infra-servicos-externos.md))
7. Completar a superfície HTTP do backend (CRUD completo de todas as entidades) e o Módulo Administrador com grupos de acesso ✅ (ver `backend/README.md`; demais telas do Administrador — Famílias/Assinaturas/Suporte/Métricas — ainda pendentes)
8. Validar com dados da família do idealizador antes de abrir para outras famílias

## Convenções

- Cadastro (categorias, tarefas, parametrização de valores/pontos, usuários) é
  exclusivo da Web — o app mobile atende apenas o usuário Comum (consulta de
  tarefas e indicação de conclusão).
- Isolamento multi-tenant é reforçado em duas camadas: filtro por `familia_id`
  na aplicação e Row Level Security no PostgreSQL.
- Integração com gateways de pagamento é sempre feita atrás da interface
  `IPaymentProvider`, nunca diretamente contra o SDK do provedor.
- **Todo frontend (Web, e qualquer outra interface) é construído via Lovable**
  — o código React do diretório `/web` é gerado/gerenciado pelo Lovable, não
  escrito manualmente fora dele.
