# Mobile Android — Mesada App

App nativo Kotlin, exclusivo para o usuário Comum (filho). Ver escopo
completo, fluxo offline-first e telas em
[`docs/adendo-mobile.md`](../docs/adendo-mobile.md).

## ⚠️ Não compilado neste ambiente de desenvolvimento

Este sandbox tem acesso à internet restrito por política de rede: o
domínio `dl.google.com` (repositório de SDKs/pacotes Android — tanto o
`sdkmanager` quanto `google()`/`maven.google.com`, que só redireciona para
ele) está bloqueado. Isso significa que nenhuma dependência AndroidX
(Compose, Room, Navigation, Biometric, `androidx.core`, etc.) pode ser
baixada aqui, e portanto **o módulo `:app` não pôde ser compilado nem
testado neste ambiente**. Isso será validado na primeira máquina de
desenvolvimento (ou CI) com acesso normal à internet — não há nada
específico deste projeto que impeça o build, é puramente uma limitação de
rede deste sandbox.

**O que FOI validado de verdade, com um compilador Kotlin real**: o módulo
`:core-logic` — que não depende do Android SDK, só de Kotlin/JVM puro — foi
compilado e teve sua lógica executada com sucesso (ver
[`core-logic/README` abaixo](#core-logic-a-parte-validada-de-verdade)).

Antes do primeiro build real do `:app`, revise `NetworkModule.kt`: o import
de `asConverterFactory` já foi corrigido uma vez neste desenvolvimento
(pacote real é `com.jakewharton.retrofit2...`, não `retrofit2...` — só
descobri isso baixando o `.jar` de fontes da biblioteca no Maven Central,
que por acaso não está bloqueado). As demais APIs usadas (Compose Material3,
Room, Navigation-Compose, Biometric) são as assinaturas estáveis e
amplamente documentadas dessas bibliotecas, mas nenhuma delas passou por um
compilador — trate o primeiro `gradle build` como a primeira revisão real
deste código, não como uma formalidade.

## Estrutura

```
mobile-android/
  core-logic/    Módulo Kotlin puro (sem Android) — fila de sincronização
                 offline e resolução de conflito. VALIDADO com kotlinc local.
  app/           Módulo Android — UI (Compose), rede (Retrofit), persistência
                 (Room), autenticação (JWT + Keychain/DataStore), biometria.
```

## `core-logic`: a parte validada de verdade

```
core-logic/src/main/kotlin/com/mesadaapp/mobile/core/
  Modelos.kt              TarefaLocal, ExecucaoPendente, StatusExecucaoLocal
  FilaSincronizacao.kt     fila offline: o que enviar, quando desistir
  ResolvedorConflito.kt    "a mudança mais recente prevalece" (docs/adendo-mobile.md)
core-logic/src/test/kotlin/...  testes kotlin.test (rodam via `gradle test`
                                 quando houver acesso à internet)
```

Sem AGP/Android SDK envolvidos, dá para rodar `gradle :core-logic:test`
(ou compilar manualmente com `kotlinc`) em qualquer máquina com JDK — foi
exatamente assim que validei esta parte aqui: copiei os três arquivos de
`src/main` para um diretório temporário, escrevi um harness sem
dependências espelhando as mesmas asserções dos testes reais, compilei com
`kotlinc` (versão antiga disponível via `apt` neste sandbox — sem acesso a
repositórios Maven) e rodei. Os 8 casos passaram.

## `app`: escrito com cuidado, mas não compilado aqui

Stack: Kotlin, Jetpack Compose (Material3), Navigation-Compose, Retrofit +
OkHttp + kotlinx.serialization, Room, DataStore (token), `androidx.biometric`.

Segue a mesma disciplina do Painel Web (Lovable, ver `web/README.md`): só a
tela **Login/Convite** fala de verdade com o backend
(`POST /api/auth/convites/{codigo}/resgatar` — o único endpoint que existe
hoje para o usuário Comum). **Minhas Tarefas** e **Detalhe da Tarefa** já
têm toda a estrutura real (Room, fila de sincronização, captura de foto),
mas exibem dados de exemplo (`TarefasRepository.carregarExemplo()`) até o
backend expor um endpoint de listagem de tarefas/execuções. **Histórico** e
**Meu Saldo** são placeholders "Em breve" — sem números inventados.

Bloqueio local após o vínculo inicial (`docs/adendo-mobile.md
#autenticação-no-app`) usa `BiometricPrompt` com
`BIOMETRIC_WEAK or DEVICE_CREDENTIAL`: biometria OU o PIN/padrão/senha que a
pessoa já configurou no aparelho — não implementamos um PIN próprio do app.

## Quando houver acesso à internet normal

```bash
cd mobile-android
./gradlew :core-logic:test   # já deve passar — é o que validei aqui
./gradlew :app:assembleDebug # primeiro build real deste módulo — revisar com atenção
```

`app/build.gradle.kts` lê a URL da API de `BuildConfig.API_BASE_URL`
(default `http://10.0.2.2:5080`, o alias do emulador Android para o
localhost da máquina host).
