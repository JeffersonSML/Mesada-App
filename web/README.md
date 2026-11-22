# Web — MesadApp

Painel web (Master + Administrativo). React 19 + TanStack Start/Router +
Tailwind v4 + shadcn/ui, mantido diretamente neste repositório (ver
[ADR 0008](../docs/adr/0008-web-mantida-no-repositorio.md)). Escopo, telas e
prints em [`docs/adendo-web.md`](../docs/adendo-web.md).

O frontend é um cliente puro da API .NET de [`/backend`](../backend): não tem
banco nem backend próprio e só guarda os tokens JWT no navegador.

## Rodando localmente

Requer [Bun](https://bun.sh) e o backend rodando (ver
["Rodando localmente"](../backend/README.md#rodando-localmente), por padrão em
`http://localhost:5080`).

```bash
cd web
bun install
echo "VITE_API_BASE_URL=http://localhost:5080" > .env
bun run dev          # http://localhost:5173
```

O CORS do backend só libera as origens de `Cors:OrigensPermitidas`
(`appsettings.json` / `Cors__OrigensPermitidas__N`); acesse por
`http://localhost:5173`, não `127.0.0.1`.

Outros comandos: `bun run build` (gera `.output/`, servidor Node),
`npx tsc --noEmit` (typecheck), `bun run lint`, `bun run format`.

## Variáveis

| Variável | Quando | Descrição |
|---|---|---|
| `VITE_API_BASE_URL` | **build** | URL pública da API. É embutida no bundle pelo Vite: mudar exige novo build. |
| `NITRO_PRESET` | build | Alvo do build. Padrão `node-server`. |
| `PORT` / `HOST` | execução | Porta/host do servidor Node (`3000` / `0.0.0.0` na imagem). |

## Deploy (Railway)

Segundo serviço no mesmo projeto do backend: Root Directory `web`, build pelo
`web/Dockerfile`, variável `VITE_API_BASE_URL` apontando para a URL pública do
backend. Depois de gerar o domínio público do Web, adicione-o ao CORS do
backend (`Cors__OrigensPermitidas__2=https://<dominio-do-web>`).

## Notas

O app mobile nunca replica telas de cadastro — apenas consome a API para
consulta de tarefas e indicação de conclusão pelo usuário Comum. Ver
[`docs/adendo-mobile.md`](../docs/adendo-mobile.md).
