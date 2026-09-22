# Web — Mesada App

Aplicação Angular, construída e deployada via Lovable.

Status: a ser criado na Etapa 4 do roadmap (ver [README raiz](../README.md)).

## Responsabilidades

Toda a superfície de cadastro do sistema é exclusiva da Web:

- Cadastro de famílias, Masters, filhos, categorias/subcategorias e tarefas
- Parametrização de valores, pontos, tipo de tarefa, aprovação e multas
- Dashboards e relatórios do Master (ver
  [`docs/especificacao.md`](../docs/especificacao.md#telas-e-dashboards-do-master-web))
- Gestão de assinatura (Master Financeiro)
- Módulo Administrador (ver
  [`docs/especificacao.md`](../docs/especificacao.md#telas-do-administrador))

## Notas

O app mobile nunca replica telas de cadastro — apenas consome a API para
consulta de tarefas e indicação de conclusão pelo usuário Comum.
