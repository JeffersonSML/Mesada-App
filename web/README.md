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

O Módulo Administrador (ver
[`docs/especificacao.md`](../docs/especificacao.md#telas-do-administrador))
vive **neste mesmo projeto Lovable**, em uma área própria (`/admin/...`)
com autenticação e dados totalmente segregados do painel do Master — ver
[ADR 0007](../docs/adr/0007-modulo-administrador-mesma-app.md). Só a
gestão de Administradores e Grupos de acesso está implementada até agora;
as demais telas previstas na spec (Famílias, Assinaturas, Suporte,
Métricas, Categorias Padrão) ainda não têm backend.

## Estado atual (Etapa 7 — Painel da Família completo + Painel Administrativo)

Duas rodadas de build do Lovable, ambas validadas de ponta a ponta rodando
o código gerado localmente (`npm install` + `vite dev`) contra o backend
.NET real (não só por leitura do código nem pelos resumos do próprio
Lovable). Stack efetiva escolhida pelo Lovable dentro de React/TypeScript:
TanStack Router (rotas), React Query (estado de chamadas à API), Tailwind
com paleta em `oklch`, componentes `shadcn/ui`.

**Painel da Família** — implementado e validado contra a API real: Login,
Dashboard, Gestão de Filhos (CRUD completo), Aprovações Pendentes, Controle
de Acessos (Convites + Masters), Extrato e Histórico (ciclos), Categorias,
Tarefas (com aderências e sugestão de valor). Como placeholder "Em breve"
(backend ainda não expõe os endpoints): Assinatura, Gráficos de Desempenho.

**Painel Administrativo (`/admin/...`)** — implementado e validado contra a
API real: Login, troca de senha obrigatória no primeiro acesso, gestão de
Administradores (convite com senha temporária, edição, desativação) e de
Grupos de acesso (Owner fixo vs. grupos customizados como "Tecnologia").
Ver [ADR 0007](../docs/adr/0007-modulo-administrador-mesma-app.md).

Essa validação local também encontrou e corrigiu dois bugs reais que só
apareceriam com o frontend de verdade batendo na API de verdade (nenhum dos
dois foi pego pelos 95 testes de integração, que chamam a API com tipos C#
diretamente): a API serializava enums como número em vez da string
documentada no contrato, e a API não tinha CORS configurado — qualquer
origem diferente da própria API (o preview do Lovable, ou este projeto
rodando localmente) teria 100% das chamadas bloqueadas pelo navegador.
Ambos corrigidos no backend.

### Prints de todas as telas

Capturados com Playwright contra o stack real (backend .NET local + build
do Lovable rodando localmente), logado de verdade — não são mockups.

**Painel da Família**

| Tela | Print |
|---|---|
| Login | ![Login](../docs/screenshots/web/01-login.png) |
| Dashboard da Família | ![Dashboard](../docs/screenshots/web/02-dashboard.png) |
| Gestão de Filhos | ![Filhos](../docs/screenshots/web/03-filhos.png) |
| Aprovações Pendentes | ![Aprovações](../docs/screenshots/web/04-aprovacoes.png) |
| Controle de Acessos | ![Acessos](../docs/screenshots/web/05-acessos.png) |
| Extrato e Histórico | ![Extrato](../docs/screenshots/web/06-extrato.png) |
| Categorias | ![Categorias](../docs/screenshots/web/07-categorias.png) |
| Tarefas | ![Tarefas](../docs/screenshots/web/08-tarefas.png) |
| Assinatura (em breve) | ![Assinatura](../docs/screenshots/web/09-assinatura.png) |
| Gráficos (em breve) | ![Gráficos](../docs/screenshots/web/10-graficos.png) |

**Painel Administrativo**

| Tela | Print |
|---|---|
| Login administrativo | ![Login admin](../docs/screenshots/web/11-admin-login.png) |
| Troca de senha obrigatória | ![Troca de senha](../docs/screenshots/web/12-admin-trocar-senha.png) |
| Administradores | ![Administradores](../docs/screenshots/web/13-admin-administradores.png) |
| Grupos de acesso | ![Grupos](../docs/screenshots/web/14-admin-grupos.png) |
| Meu perfil (Owner) | ![Perfil](../docs/screenshots/web/15-admin-perfil.png) |

Nota sobre o print de "Aprovações Pendentes": o campo que identifica a
tarefa/filho ainda mostra um identificador técnico em vez do nome — a API
já foi corrigida para enviar `nomeTarefa`/`nomeFilho`, falta só a mensagem
de ajuste ser enviada ao Lovable para consumir os campos novos (bloqueado
no momento por falta de créditos no workspace Lovable).

## Notas

O app mobile nunca replica telas de cadastro — apenas consome a API para
consulta de tarefas e indicação de conclusão pelo usuário Comum.
