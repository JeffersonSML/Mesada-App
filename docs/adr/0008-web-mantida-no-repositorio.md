# ADR 0008: Web mantida diretamente neste repositório (substitui o ADR 0004)

- Status: aceito
- Data: 2026-11-22
- Substitui: [ADR 0004](0004-web-via-lovable.md)

## Contexto

O ADR 0004 definiu que todo o frontend seria gerado e mantido pelo Lovable,
com o código vivendo só no projeto Lovable. Na prática isso trouxe atritos:
cada ajuste visual passava por um ciclo de mensagem/espera/validação, o
workspace ficou sem créditos duas vezes (travando trocas simples, como a de
fonte), o contrato de API precisava ser copiado à mão para a "Project
Knowledge" do Lovable, e as telas resultantes foram avaliadas como simples
demais pelo responsável do produto.

## Decisão

O frontend passa a ser desenvolvido diretamente neste repositório, em
`/web`, com o Claude Code. O Lovable deixa de ser usado.

- **Ponto de partida:** exportação do projeto Lovable no commit `3422806`
  (último estável), sem alterações de conteúdo. Typecheck, build e um fluxo
  real (cadastro → dashboard → telas) foram validados contra o backend local
  antes de qualquer mudança.
- **Stack mantida** para reaproveitar tudo: React 19, TanStack Start/Router,
  Tailwind v4, shadcn/ui, Vite.
- **Desacoplamento do Lovable:** removidos o pacote proprietário
  `@lovable.dev/vite-tanstack-config` (substituído por uma `vite.config.ts`
  explícita) e o gancho de relato de erros que só funcionava dentro da
  plataforma. O build agora usa o preset `node-server` do Nitro (e não mais
  Cloudflare Workers), configurável via `NITRO_PRESET`.
- **Deploy:** segundo serviço na Railway, com Root Directory `web` e o
  `web/Dockerfile`. `VITE_API_BASE_URL` é variável de **build** (o Vite a
  embute no bundle).

## Consequências

- Backend e frontend passam a evoluir no mesmo commit; mudanças de contrato
  de API podem ser feitas e consumidas juntas, sem sincronização manual.
- O CORS do backend (`Cors__OrigensPermitidas`) precisa listar a URL pública
  do serviço Web.
- O projeto Lovable fica como arquivo morto (somente leitura); não recebe
  mais mensagens.
- O `web/Dockerfile` ainda não foi construído com Docker neste ambiente de
  desenvolvimento (sem daemon); o build do Vite e o servidor Node de produção
  foram validados diretamente.
