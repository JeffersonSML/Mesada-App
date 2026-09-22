# ADR 0004: Web construída via Lovable (React), não Angular

- Status: aceito
- Data: 2026-09-22

## Contexto

A especificação original (`docs/especificacao.md#stack-técnica-e-arquitetura`)
previa Angular para a Web, "build/deploy via Lovable". Ao iniciar a Etapa 4,
o responsável pelo produto instruiu explicitamente: "tudo relacionado a
frontend, deve ser utilizado o Lovable". A ferramenta Lovable (MCP), porém,
não oferece Angular como opção — todo projeto criado por ela usa sua stack
própria: **React + TypeScript + Tailwind CSS + shadcn/ui**, com Supabase
como backend opcional.

## Decisão

A Web é construída inteiramente através do Lovable, na stack real dele
(React/TypeScript), não em Angular escrito manualmente. O código do projeto
Lovable não é versionado neste repositório — ele vive no próprio Lovable
(link em `web/README.md`); este repositório documenta o contrato entre
frontend e backend e o estado das telas.

**Supabase/Lovable Cloud desativado deliberadamente.** O produto já tem um
backend próprio (`.NET` + PostgreSQL + RLS, Etapas 2–3) — usar o banco do
Lovable criaria uma segunda fonte de dados, duplicando `familia_id`,
autenticação e o motor de cálculo. O projeto Lovable é tratado como cliente
puro de API: toda leitura/escrita passa pela API REST existente via `fetch`,
nunca por uma tabela criada dentro do Lovable.

## Justificativa

- A instrução do produto sobre usar Lovable é mais específica e recente que
  a escolha de framework original — Angular era um meio para o fim de "ter
  uma Web funcional", não um requisito de negócio.
- Lovable não sabe gerar Angular; insistir nele exigiria escrever a Web
  manualmente fora do Lovable, contradizendo a instrução.
- React + shadcn/ui é uma stack madura e comum para SaaS — não há perda de
  capacidade, apenas mudança de framework.

## Consequências

- A linha "Web" da tabela de stack técnica em `README.md` reflete Lovable/
  React, não mais Angular — a especificação original (`docs/especificacao.md`)
  é mantida como está (fonte histórica), mas este ADR é a referência para a
  divergência.
- Qualquer nova tela ou alteração da Web deve ser feita enviando uma
  mensagem ao projeto Lovable (`mcp__Lovable__send_message`), nunca editando
  arquivos React manualmente neste repositório.
- A Project Knowledge do projeto Lovable precisa ser mantida em sincronia
  manual com o contrato real da API (`backend/src/Mesada.Api/Controllers`)
  toda vez que um endpoint novo for criado — não há verificação automática
  disso ainda.
- Enquanto o backend só roda localmente neste ambiente de desenvolvimento
  (não há deploy público — isso é Etapa 6), o preview do Lovable não
  consegue de fato chamar a API; as telas que já consomem endpoints reais
  (Gestão de Filhos, Dashboard) só serão validadas em produção quando o
  backend for deployado.
