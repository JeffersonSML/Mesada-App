# Web — Mesada App

Painel web do Master (pai/mãe/responsável), construído e mantido via
[Lovable](https://lovable.dev) — código gerado/editado pelo Lovable, não
escrito manualmente neste diretório.

**Projeto Lovable:** https://lovable.dev/projects/0ffe502c-7386-4b57-9954-c12b45b20cb9
**Preview:** https://id-preview--0ffe502c-7386-4b57-9954-c12b45b20cb9.lovable.app

## Nota sobre a stack

A especificação original previa Angular para a Web. Lovable não gera
projetos Angular — sua stack padrão é **React + TypeScript + Tailwind +
shadcn/ui**. Por instrução explícita do responsável pelo produto ("tudo
relacionado a frontend deve usar o Lovable"), a Web foi construída na stack
real do Lovable em vez de Angular escrito manualmente. Ver
[`docs/adr/0004-web-via-lovable.md`](../docs/adr/0004-web-via-lovable.md).

## Arquitetura: frontend puro, sem backend próprio

Este projeto **não** usa Supabase/Lovable Cloud nem tem banco de dados
próprio — toda persistência de dados é feita pela API .NET já existente em
`/backend`. O projeto Lovable consome essa API via `fetch`, nunca guarda
estado de negócio localmente além do token JWT.

- Base URL da API: variável de ambiente `VITE_API_BASE_URL` (fallback
  `http://localhost:5080` em desenvolvimento).
- Autenticação: `POST /api/auth/master/login` → JWT guardado e enviado como
  `Authorization: Bearer <token>` em toda chamada autenticada.
- O contrato completo consumido pelo frontend está registrado como
  "Project Knowledge" no próprio projeto Lovable (mantido sincronizado com
  `backend/src/Mesada.Api/Controllers`).

## Responsabilidades (visão completa, spec)

Toda a superfície de cadastro do sistema é exclusiva da Web:

- Cadastro de famílias, Masters, filhos, categorias/subcategorias e tarefas
- Parametrização de valores, pontos, tipo de tarefa, aprovação e multas
- Dashboards e relatórios do Master (ver
  [`docs/especificacao.md`](../docs/especificacao.md#telas-e-dashboards-do-master-web))
- Gestão de assinatura (Master Financeiro)

O módulo Administrador (ver
[`docs/especificacao.md`](../docs/especificacao.md#telas-do-administrador))
é tratado como um sistema/projeto Lovable separado, não faz parte deste
painel do Master.

## Estado atual (Etapa 4 — concluída)

Build inicial do Lovable finalizada (`commit ececf747`). Stack efetiva
escolhida pelo próprio Lovable dentro de React/TypeScript: TanStack Router
(rotas), React Query (estado de chamadas à API), Tailwind com paleta em
`oklch`, componentes `shadcn/ui`.

Implementado, consumindo a API real:
- Login do Master (`POST /api/auth/master/login`, JWT em `localStorage`,
  redirecionamento automático para telas protegidas)
- Layout autenticado responsivo (navegação lateral no desktop, menu
  suspenso no mobile) com toda a navegação da spec, tema claro/escuro
- Gestão de Filhos (lista real via `GET /api/filhos`, destaque visual
  quando `saldoDevedorAcumulado > 0`)
- Dashboard da Família (cards de resumo por filho reaproveitando
  `/api/filhos`, seção de tarefas pendentes como placeholder)

Como placeholder "Em breve" (backend ainda não expõe os endpoints):
Aprovações Pendentes, Extrato e Histórico, Gráficos de Desempenho, Controle
de Acessos, Assinatura. Conforme os endpoints correspondentes forem criados
no backend, a Project Knowledge do Lovable deve ser atualizada e uma nova
mensagem enviada ao projeto para implementar a tela de verdade.

**Ainda não validado neste ambiente:** o preview do Lovable rodando contra o
backend real — o `.NET` só está acessível localmente neste ambiente de
desenvolvimento (sem deploy público, isso é Etapa 6), então o login e a
listagem de filhos não puderam ser testados fim a fim contra o preview
hospedado. A lógica do fluxo foi revisada por leitura, não por execução.

## Notas

O app mobile nunca replica telas de cadastro — apenas consome a API para
consulta de tarefas e indicação de conclusão pelo usuário Comum.
