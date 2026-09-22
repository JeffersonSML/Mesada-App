# ADR 0001: Mono-repo para backend, web e mobile

- Status: aceito
- Data: 2026-09-22

## Contexto

O projeto tem quatro componentes de código (backend .NET, web Angular, app
Android nativo, app iOS nativo) além de infraestrutura e documentação, todos
construídos sequencialmente por um único desenvolvedor, seguindo o roadmap
descrito no README raiz.

## Decisão

Adotar um único repositório (`JeffersonSML/Mesada-App`) com um diretório por
componente (`/backend`, `/web`, `/mobile-android`, `/mobile-ios`, `/infra`,
`/docs`), em vez de repositórios separados por stack.

## Justificativa

- O contrato de API, o schema de dados e a especificação funcional evoluem
  juntos nesta fase e precisam ficar visíveis e versionados no mesmo lugar.
- Mudanças que cruzam camadas (ex.: novo campo em Tarefa que afeta backend,
  web e apps mobile) ficam em um único PR, mais fáceis de revisar e reverter.
- Com um único desenvolvedor construindo tudo sequencialmente (Etapas 1–7 do
  roadmap), não há benefício de isolamento de acesso por repositório ainda.

## Consequências

- CI precisa ser particionado por pasta alterada (path filters) para não
  rodar builds de todas as stacks a cada commit.
- Se o projeto crescer e passar a ter times dedicados por plataforma, este
  ADR deve ser revisitado — o histórico de cada componente pode ser extraído
  para um repositório próprio com `git filter-repo` sem perda de contexto.
