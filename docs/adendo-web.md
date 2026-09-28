# Adendo Web (Painel do Master + Painel Administrativo)

> Complementa [`especificacao.md`](especificacao.md) — não repete o que já
> está lá (telas planejadas, lógica de cálculo, modelo de dados). Aqui: o
> que é específico da Web como plataforma, e o estado real de implementação.

## Escopo

A Web é a única superfície de cadastro do sistema — famílias, Masters,
filhos, categorias/subcategorias, tarefas e toda a parametrização de
valores/pontos/aprovação/multas. O app mobile nunca replica essas telas.

Duas áreas vivem no mesmo projeto React, com autenticação e dados
totalmente segregados entre si (nunca compartilham token, chave de
`localStorage` ou navegação):

1. **Painel do Master** (`/dashboard`, `/filhos`, etc.) — pai/mãe/responsável.
2. **Painel Administrativo** (`/admin/...`) — equipe interna do produto
   (Owner + grupos convidados, ex. "Tecnologia"). Ver
   [ADR 0007](adr/0007-modulo-administrador-mesma-app.md) para os detalhes
   de segregação (claims de JWT, tabelas, RLS).

## Por que React e não Angular

A especificação original previa Angular. O motivo de a Web ser React +
TypeScript + Tailwind + shadcn/ui em vez disso: é a stack padrão do
Lovable, e a decisão do produto foi "todo frontend é construído via
Lovable" — ver [ADR 0004](adr/0004-web-via-lovable.md).

## Arquitetura: frontend puro, sem backend próprio

Este projeto não usa Supabase/Lovable Cloud nem banco de dados próprio —
toda persistência é feita pela API .NET (`/backend`), consumida via `fetch`.
Nenhum estado de negócio é guardado localmente além do(s) token(s) JWT
(um para cada uma das duas áreas, em chaves de `localStorage` diferentes).

- Base URL da API: `VITE_API_BASE_URL` (fallback `http://localhost:5080`).
- Autenticação Master: `POST /api/auth/master/login` → JWT em
  `Authorization: Bearer <token>`.
- Autenticação Administrador: `POST /api/admin/auth/login` → JWT próprio,
  com fluxo de troca de senha obrigatória no primeiro acesso
  (`deveTrocarSenha`).
- O contrato completo consumido pelo frontend é mantido como "Project
  Knowledge" no próprio projeto Lovable, sincronizado manualmente com
  `backend/src/Mesada.Api/Controllers` a cada mudança de API.

## Grupos de acesso do Painel Administrativo

Documentado em detalhe em [`especificacao.md`](especificacao.md#grupos-de-administrador-controle-de-acesso-interno).
Resumo para quem só olha este adendo: grupo "Owner" fixo com acesso total;
grupos customizados (ex. "Tecnologia") com um mapa de permissões livre que,
**hoje, ainda não é lido por nenhum endpoint** — a única distinção de
acesso realmente aplicada é Owner vs. não-Owner.

## Estado de Implementação

Build feito e validado em duas rodadas pelo agente do Lovable, com
validação end-to-end real (backend .NET local + o código gerado pelo
Lovable rodando localmente via `npm install && vite dev`, um contra o
outro) — não apenas leitura de código nem confiança nos resumos do próprio
Lovable.

**Painel do Master — implementado e validado contra a API real:**
Criar conta (cadastro da família), Login, Dashboard, Gestão de Filhos (CRUD
completo), Aprovações Pendentes, Controle de Acessos (Convites + Masters),
Extrato e Histórico (ciclos), Categorias, Tarefas (com aderências e
sugestão de valor), Notificações (destinatários extra de alerta).

**Painel do Master — ainda "Em breve" (backend não expõe os endpoints):**
Assinatura, Gráficos de Desempenho.

**Painel Administrativo — implementado e validado contra a API real:**
Login, troca de senha obrigatória no primeiro acesso, gestão de
Administradores (convite com senha temporária, edição, desativação) e de
Grupos de acesso.

**Painel Administrativo — ainda sem tela nem backend:** Famílias,
Assinaturas, Suporte, Métricas, Categorias Padrão.

### Bugs reais encontrados nessa validação (e já corrigidos)

Nenhum dos dois foi pego pelos testes de integração do backend, que batem
na API com tipos C# diretamente em vez de JSON de verdade:

- **Enums serializados como número em vez de string** — o contrato
  documentado (e o que o frontend real envia/espera) sempre usa string.
  Corrigido com `JsonStringEnumConverter` em `Program.cs`.
- **CORS não configurado** — qualquer frontend em outra origem (o preview
  hospedado do Lovable, ou este projeto rodando localmente) tinha toda
  chamada bloqueada pelo navegador antes de chegar à API. Corrigido com
  uma policy de CORS configurável (`Cors:OrigensPermitidas`).

### Pendência conhecida

A tela de Aprovações Pendentes ainda mostra um identificador técnico
(`tarefaUsuarioId`) em vez do nome do filho/tarefa — a API já foi corrigida
para enviar `nomeTarefa`/`nomeFilho` em `GET /api/execucoes/pendentes`,
falta só enviar a mensagem de ajuste ao agente do Lovable (bloqueado no
momento por falta de créditos no workspace).

## Prints de todas as telas

Capturados com Playwright contra o stack real (backend .NET local + build
do Lovable rodando localmente), logado de verdade — não são mockups.

**Painel do Master**

| Tela | Print |
|---|---|
| Criar conta (cadastro da família) | ![Cadastro](screenshots/web/16-cadastro.png) |
| Login | ![Login](screenshots/web/01-login.png) |
| Dashboard da Família | ![Dashboard](screenshots/web/02-dashboard.png) |
| Gestão de Filhos | ![Filhos](screenshots/web/03-filhos.png) |
| Aprovações Pendentes | ![Aprovações](screenshots/web/04-aprovacoes.png) |
| Controle de Acessos | ![Acessos](screenshots/web/05-acessos.png) |
| Extrato e Histórico | ![Extrato](screenshots/web/06-extrato.png) |
| Categorias | ![Categorias](screenshots/web/07-categorias.png) |
| Tarefas | ![Tarefas](screenshots/web/08-tarefas.png) |
| Notificações | ![Notificações](screenshots/web/20-notificacoes-lista.png) |
| Assinatura (em breve) | ![Assinatura](screenshots/web/09-assinatura.png) |
| Gráficos (em breve) | ![Gráficos](screenshots/web/10-graficos.png) |

**Painel Administrativo**

| Tela | Print |
|---|---|
| Login administrativo | ![Login admin](screenshots/web/11-admin-login.png) |
| Troca de senha obrigatória | ![Troca de senha](screenshots/web/12-admin-trocar-senha.png) |
| Administradores | ![Administradores](screenshots/web/13-admin-administradores.png) |
| Grupos de acesso | ![Grupos](screenshots/web/14-admin-grupos.png) |
| Meu perfil (Owner) | ![Perfil](screenshots/web/15-admin-perfil.png) |

## Manual operacional (para uso, não para manutenção)

Este adendo é documentação técnica — para uma pessoa entender como usar
cada tela, passo a passo, com print de cada uma explicando como operar,
existe um manual operacional em PDF, gerado a partir do código-fonte em
[`docs/manual-operacional/`](manual-operacional/) (o PDF em si não é
versionado, por ser artefato de build — ver
[`docs/manual-operacional/README.md`](manual-operacional/README.md) para
regerar).

## Links do projeto Lovable

- **Projeto:** https://lovable.dev/projects/0ffe502c-7386-4b57-9954-c12b45b20cb9
- **Preview:** https://id-preview--0ffe502c-7386-4b57-9954-c12b45b20cb9.lovable.app

Como rodar/testar (localmente ou via deploy público): ver
[`web/README.md`](../web/README.md).
