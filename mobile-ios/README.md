# Mobile iOS — Mesada App

App nativo Swift, exclusivo para o usuário Comum (filho).

Status: a ser criado na Etapa 5 do roadmap (ver [README raiz](../README.md)).

Ver escopo completo, fluxo offline-first e telas em
[`docs/adendo-mobile.md`](../docs/adendo-mobile.md).

## Responsabilidades

- Consulta de tarefas do ciclo atual e histórico
- Indicação de conclusão (feito/parcial/não feito) dentro do prazo
- Captura de evidência (foto) e integrações futuras (Strava, Apple HealthKit)
- Autenticação por código/link de convite + PIN/biometria local (Face ID/Touch ID)
- Offline-first: SQLite local (ou Core Data) + fila de sincronização
- Notificações push via Firebase Cloud Messaging (ou APNs)

Fora do escopo: qualquer tela de cadastro (exclusivo da Web).
