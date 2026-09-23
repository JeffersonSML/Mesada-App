# ADR 0005: Apps mobile — restrições deste ambiente e como validei o que dava

- Status: aceito
- Data: 2026-09-23

## Contexto

Etapa 5 do roadmap pede os projetos mobile nativos (Android/Kotlin,
iOS/Swift). Diferente das Etapas 2–4, este ambiente de desenvolvimento
(sandbox Linux, sem GUI, rede corporativa restrita) tem limitações reais que
impedem validar os dois projetos com o mesmo rigor usado no backend (onde
rodei migrações e testes de integração contra um PostgreSQL real) e no
Painel Web (onde o próprio Lovable compilou e hospedou o preview).

## O que descobri, nesta ordem

1. `dl.google.com` está bloqueado pela política de rede do sandbox
   (`curl` retorna `CONNECT tunnel failed, response 403`). Esse domínio serve
   tanto o repositório do `sdkmanager` (platforms, build-tools) quanto os
   binários que `google()`/`maven.google.com` resolvem no Gradle.
2. `maven.google.com` responde 200, mas **todo artefato nele é um redirect
   301 para `dl.google.com`** — confirmei baixando o `.jar` de fontes do
   `androidx.biometric:biometric:1.1.0` e vendo o `location:` do redirect.
   Ou seja, não existe forma de contornar o bloqueio trocando a URL do
   repositório: nenhuma dependência `androidx.*` pode ser baixada aqui.
3. `repo1.maven.org` (Maven Central) funciona (com rate-limiting ocasional,
   HTTP 429, que passa numa segunda tentativa) — bibliotecas não-Google
   (Retrofit, OkHttp, kotlinx.serialization, o próprio Kotlin) são
   alcançáveis.
4. `download.swift.org` (toolchain oficial do Swift) também está bloqueado.
   Mesmo a parte de um projeto iOS que é portável (Swift puro, sem
   UIKit/SwiftUI, que em tese compilaria em Linux) não pôde ser compilada
   aqui — só revisada manualmente.
5. Via `apt`, este Ubuntu tem pacotes reais de Android (`android.jar` da API
   23, `aapt`, `aapt2`, `zipalign`, `apksigner`) — mas **sem `d8`/`dx`** (o
   dexer), e só compileSdk 23, muito abaixo do que um projeto Android atual
   (compileSdk 34) exige. Não é uma base viável para reproduzir o build
   real do projeto.

Conclusão: **nem o Android nem o iOS puderam ser compilados/testados de
ponta a ponta neste ambiente** — isso é uma limitação de infraestrutura
deste sandbox, não do código em si, e vai desaparecer na primeira máquina
com acesso à internet normal (Android) ou um Mac com Xcode (iOS).

## Decisão: separar o que é lógica pura do que é UI/SDK, e validar o que der

Em vez de escrever os dois apps inteiros sem nenhuma validação, separei
explicitamente a lógica de negócio testável (fila de sincronização offline,
resolução de conflito por timestamp — a parte mais fácil de errar
silenciosamente) num módulo sem dependência de Android SDK nem
UIKit/SwiftUI:

- `mobile-android/core-logic`: Kotlin puro. **Validado de verdade**: copiei
  os arquivos para fora do projeto, escrevi um harness com as mesmas
  asserções dos testes reais (sem framework de teste, já que o `kotlinc`
  disponível via `apt` é a versão 1.3.31, antiga demais para depender de
  Maven), compilei e rodei — os 8 casos passaram. Os testes "de verdade"
  (`kotlin.test`) ficam no módulo, para rodar via `gradle test` assim que
  houver rede.
- `mobile-ios/MesadaCore`: Swift Package equivalente, tradução linha-a-linha
  do módulo Kotlin (mesmos nomes, mesma regra). Não pôde ser compilado
  (toolchain Swift bloqueada), mas carrega o mesmo raciocínio já validado do
  lado Android — o risco residual é só de sintaxe Swift, não de lógica.

Para o restante de cada app (UI, rede, persistência, biometria/Keychain),
usei o Maven Central acessível para verificar pelo menos as APIs de
terceiros mais arriscadas: baixei o `.jar` de fontes do
`retrofit2-kotlinx-serialization-converter` e descobri que o pacote real é
`com.jakewharton.retrofit2.converter.kotlinx.serialization`, não
`retrofit2.converter.kotlinx.serialization` como eu tinha escrito — um erro
real que só apareceria num build de verdade, corrigido antes de commitar.

## Consequências

- `mobile-android/README.md` e `mobile-ios/README.md` marcam claramente o
  que foi validado (core-logic/MesadaCore) e o que não foi (o resto),
  para quem pegar isso depois não confundir "está escrito com cuidado" com
  "já rodou".
- O primeiro `gradle build` do módulo `:app` (numa máquina com internet
  normal) e a primeira abertura do projeto Xcode (num Mac) devem ser
  tratados como a primeira revisão real desse código — reservar tempo para
  isso, não assumir que só falta "rodar".
- Se este produto vier a precisar de CI mobile, o runner precisa ter acesso
  irrestrito a `dl.google.com`/`maven.google.com` (Android) e rodar em
  macOS com Xcode (iOS) — nenhuma das duas restrições que bati aqui deve se
  repetir num ambiente de CI padrão (GitHub Actions oferece runners macOS
  com Xcode pré-instalado, por exemplo).
