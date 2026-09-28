# Manual Operacional — código-fonte

Fonte do manual operacional em PDF (guia funcional de uso de cada tela,
passo a passo, com print de cada uma) pedido explicitamente pelo
responsável do produto, para não misturar documentação de uso com a
documentação técnica em Markdown (`docs/especificacao.md`,
`docs/adendo-web.md`, `docs/adendo-mobile.md`) usada para manutenção e
evolução do código.

- `manual.html` — conteúdo do manual (HTML com CSS de impressão), referencia
  os prints em `../screenshots/web/` por caminho relativo.
- `gerar-pdf.mjs` — renderiza `manual.html` para PDF via Playwright
  (`chromium`), com numeração de página automática.

## Como gerar o PDF

Requer Playwright instalado (não é dependência do projeto — instale à
parte):

```bash
npm install playwright
node docs/manual-operacional/gerar-pdf.mjs
```

Gera `docs/manual-operacional/Manual-Operacional-Mesada-App.pdf`
(ignorado pelo git — é artefato de build, não fonte).

## Quando atualizar

Sempre que uma tela mudar de verdade (novo campo, fluxo diferente, tela
nova), atualize `manual.html` e recapture o(s) print(s) relevante(s) em
`../screenshots/web/` antes de regenerar o PDF — o manual só tem valor se
refletir o app real, não o planejado.
