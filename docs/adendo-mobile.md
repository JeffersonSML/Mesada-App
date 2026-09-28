# Adendo Mobile (Android/iOS)

> Cópia versionada do adendo mobile da especificação funcional. Fonte editável
> (documento vivo): https://claude.ai/artifact/EW6oah9zbQ9syam7WUaypL

## Escopo do App

O app mobile (Android e iOS) atende exclusivamente o usuário Comum (filho).
Funcionalidades: consultar tarefas atribuídas e seu histórico, e indicar
conclusão (feito/parcial/não feito) de tarefas dentro do prazo.

Fora do escopo do app: qualquer cadastro (usuários, categorias, tarefas,
parametrização de valores/pontos) — exclusivo da Web.

## Fluxo Offline-first e Sincronização

Offline é essencial: o filho pode marcar uma tarefa como concluída sem
conexão. Dados ficam em SQLite local e entram em uma fila de sincronização,
enviada ao backend assim que houver conexão.

Conflitos (ex.: tarefa alterada na Web enquanto o app estava offline)
resolvidos por timestamp — a mudança mais recente prevalece, com o Master
notificado se um conflito de valor ocorrer.

## Captura de Evidência

Foto obrigatória por padrão para tarefas sem integração automática. Fotos
armazenadas em storage compatível com S3, vinculadas à Execução da tarefa.

Integrações futuras: Strava (evidência automática de atividades esportivas) e
Google Fit/Apple HealthKit (evidência genérica de treino/passos), reservadas
ao plano Premium.

## Autenticação no App

O filho não tem e-mail nem documento cadastrado. Acesso via código/link de
convite gerado pelo Master na Web, inserido no app no primeiro uso e
vinculado ao dispositivo.

Após o vínculo inicial, recomenda-se bloqueio local (PIN definido no app ou
biometria do aparelho) para o acesso diário, dado que o app expõe dados
financeiros da criança.

## Notificações Push

Via Firebase Cloud Messaging. Eventos relevantes ao filho: tarefa pendente,
tarefa aprovada/rejeitada, mesada fechada, tarefa avulsa expirando.

## Telas Principais

| Tela | Conteúdo |
|---|---|
| Login/Convite | Inserção do código de convite; configuração de PIN/biometria |
| Minhas Tarefas | Lista de tarefas do ciclo atual, com status (pendente/feito/parcial/não feito) |
| Detalhe da Tarefa | Descrição, prazo, pontos/valor, campo de evidência (foto ou integração), botão de conclusão |
| Histórico | Tarefas de ciclos anteriores e extrato simplificado da mesada |
| Meu Saldo | Valor do ciclo atual, saldo devedor acumulado (se houver), próxima data de fechamento |

## Estado de Implementação

Mesma disciplina do Painel Web (ver [`adendo-web.md`](adendo-web.md)): só a
tela **Login/Convite** fala de verdade com o backend hoje
(`POST /api/auth/convites/{codigo}/resgatar` — único endpoint que existe
para o usuário Comum). **Minhas Tarefas** e **Detalhe da Tarefa** já têm
toda a estrutura real (persistência local, fila de sincronização, captura
de foto), mas exibem dados de exemplo até o backend expor um endpoint de
listagem de tarefas/execuções para o Comum. **Histórico** e **Meu Saldo**
são placeholders "Em breve" — sem números inventados.

Diferente da Web, nenhum dos dois apps foi compilado/rodado de verdade
neste ambiente de desenvolvimento — limitações de rede e de plataforma
(não há macOS disponível para iOS), não do código em si:

- **Android**: o módulo `core-logic` (lógica pura, sem Android SDK) foi
  compilado e testado de verdade com `kotlinc`. O módulo `app` (Compose,
  Room, Retrofit) não pôde ser compilado aqui — domínio de dependências
  AndroidX bloqueado por política de rede do sandbox. Detalhes em
  [`mobile-android/README.md`](../mobile-android/README.md).
- **iOS**: nenhum arquivo foi compilado — Swift/Xcode exigem macOS, que não
  existe neste ambiente. Código-fonte organizado e um `Package.swift` para
  a parte portável (`MesadaCore`, tradução 1:1 do `core-logic` Android),
  prontos para virar um projeto Xcode na primeira máquina com Mac. Detalhes
  em [`mobile-ios/README.md`](../mobile-ios/README.md).

Nenhum print de tela: sem build rodando, não há preview real para capturar.
