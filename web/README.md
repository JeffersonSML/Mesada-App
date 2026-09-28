# Web — Mesada App

Painel web (Master + Administrativo), construído e mantido via
[Lovable](https://lovable.dev) — código gerado/editado pelo Lovable, não
escrito manualmente neste diretório. Escopo completo, decisões de stack,
estado de implementação e prints de todas as telas em
[`docs/adendo-web.md`](../docs/adendo-web.md).

**Projeto Lovable:** https://lovable.dev/projects/0ffe502c-7386-4b57-9954-c12b45b20cb9
**Preview:** https://id-preview--0ffe502c-7386-4b57-9954-c12b45b20cb9.lovable.app

> ⚠️ **O preview hospedado acima ainda não funciona sozinho.** Ele roda no
> seu navegador e tenta chamar a API em `http://localhost:5080` (o padrão de
> `VITE_API_BASE_URL`) — ou seja, a porta 5080 da **sua própria máquina**,
> onde nada está rodando. O backend .NET ainda não foi publicado num
> endereço público (isso é a Etapa 6, deploy, ainda não feita por decisão
> do responsável do produto). Até lá, "Não foi possível conectar à API" no
> preview hospedado é esperado — não é senha errada nem bug do frontend.
> Para testar de verdade, rode o backend e a Web localmente na mesma
> máquina (ver "Como testar localmente" abaixo).

## Arquitetura: frontend puro, sem backend próprio

Este projeto **não** usa Supabase/Lovable Cloud nem tem banco de dados
próprio — toda persistência de dados é feita pela API .NET já existente em
`/backend`. O projeto Lovable consome essa API via `fetch`, nunca guarda
estado de negócio localmente além do(s) token(s) JWT.

- Base URL da API: variável de ambiente `VITE_API_BASE_URL` (fallback
  `http://localhost:5080` em desenvolvimento).
- O contrato completo consumido pelo frontend está registrado como
  "Project Knowledge" no próprio projeto Lovable (mantido sincronizado com
  `backend/src/Mesada.Api/Controllers`).

## Como testar localmente

Com o preview hospedado ainda sem backend público (ver aviso no topo), o
jeito de testar de ponta a ponta hoje é rodar os dois lados na mesma
máquina:

1. **Backend** — siga ["Rodando localmente"](../backend/README.md#rodando-localmente)
   no README do backend (provisionar Postgres, exportar as variáveis de
   ambiente, `dotnet run --project src/Mesada.Api`). Por padrão sobe em
   `http://localhost:5080`.
2. **Web** — baixe o código deste projeto Lovable (editor → menu do
   projeto → "Export"/Dev Mode, ou `git clone` se o Lovable já estiver
   sincronizado com um repositório Git):
   ```bash
   npm install
   echo "VITE_API_BASE_URL=http://localhost:5080" > .env
   npm run dev
   ```
   Abre em `http://localhost:5173` (ou a porta que o Vite escolher), já
   falando com o backend local.

Publicar o backend num endereço público (para o preview hospedado do
Lovable funcionar sozinho, sem precisar rodar nada localmente) é uma
decisão de infraestrutura em aberto — avise quando quiser seguir com isso.

## Notas

O app mobile nunca replica telas de cadastro — apenas consome a API para
consulta de tarefas e indicação de conclusão pelo usuário Comum. Ver
[`docs/adendo-mobile.md`](../docs/adendo-mobile.md).
