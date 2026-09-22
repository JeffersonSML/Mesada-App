# Mobile Android — Mesada App

App nativo Kotlin, exclusivo para o usuário Comum (filho).

Status: a ser criado na Etapa 5 do roadmap (ver [README raiz](../README.md)).

Ver escopo completo, fluxo offline-first e telas em
[`docs/adendo-mobile.md`](../docs/adendo-mobile.md).

## Responsabilidades

- Consulta de tarefas do ciclo atual e histórico
- Indicação de conclusão (feito/parcial/não feito) dentro do prazo
- Captura de evidência (foto) e integrações futuras (Strava, Google Fit)
- Autenticação por código/link de convite + PIN/biometria local
- Offline-first: SQLite local + fila de sincronização
- Notificações push via Firebase Cloud Messaging

Fora do escopo: qualquer tela de cadastro (exclusivo da Web).
