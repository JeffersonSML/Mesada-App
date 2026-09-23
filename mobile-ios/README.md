# Mobile iOS — Mesada App

App nativo Swift, exclusivo para o usuário Comum (filho). Ver escopo
completo, fluxo offline-first e telas em
[`docs/adendo-mobile.md`](../docs/adendo-mobile.md).

## ⚠️ Não compilado — e não há como compilar fora de um Mac

Diferente do Android (onde a limitação é só a rede deste sandbox), Swift
para iOS **exige Xcode rodando em macOS** — não existe toolchain
oficial para compilar/rodar um app iOS em Linux, com ou sem internet.
Confirmei também que o domínio de download do toolchain Swift oficial
(`download.swift.org`) está bloqueado neste sandbox, então nem a parte
*portável* (Swift puro, sem UIKit/SwiftUI) pôde ser compilada aqui — só
revisada manualmente, com bastante cuidado.

**Nenhum arquivo `.xcodeproj` foi criado.** Um projeto Xcode é um pacote
binário/XML fácil de corromper sem o próprio Xcode para gerar e validar; ao
invés de arriscar entregar um `.xcodeproj` que abre quebrado — pior do que
não ter nenhum — este diretório traz o código-fonte Swift organizado do
jeito que o Xcode espera, mais um `Package.swift` para a parte que é
portável. Ver "Próximo passo" abaixo para transformar isso num projeto
Xcode de verdade, em poucos minutos, na primeira máquina com Mac.

## Estrutura

```
mobile-ios/
  MesadaCore/              Swift Package — lógica pura (Foundation apenas,
                            sem UIKit/SwiftUI). Espelha 1:1
                            mobile-android/core-logic, mesmos nomes e regras.
    Sources/MesadaCore/     Modelos, FilaSincronizacao, ResolvedorConflito
    Tests/MesadaCoreTests/  XCTest com os MESMOS casos de
                            mobile-android/core-logic/src/test — não rodados
                            aqui (toolchain Swift indisponível), mas prontos.
  MesadaApp/                Código-fonte do app — SwiftUI, Networking,
                            Persistence. Não é um target buildável por si só
                            aqui; precisa ser adicionado a um projeto Xcode.
```

## `MesadaApp`: mesma disciplina do Android e do Painel Web

Só **LoginConviteView** fala de verdade com o backend (resgate de convite).
**MinhasTarefasView** e **DetalheTarefaView** (com captura de foto via
`UIImagePickerController`) já têm toda a estrutura real — persistência local
em JSON (`ArmazenamentoLocal`, equivalente ao Room do Android) e fila de
sincronização (`MesadaCore.FilaSincronizacao`) — mas exibem dados de exemplo
até o backend expor um endpoint de listagem de tarefas. **HistoricoView** e
**MeuSaldoView** são placeholders "Em breve".

Token JWT guardado no Keychain (`TokenStore.swift`), nunca em
`UserDefaults`. `Info.plist.reference.xml` documenta as chaves
(`NSCameraUsageDescription`, `NSFaceIDUsageDescription`) que o Info.plist
real do target vai precisar.

## Próximo passo: criar o projeto Xcode (num Mac)

1. Xcode → File → New → Project → App. Bundle identifier
   `com.mesadaapp.mobile`, interface SwiftUI, linguagem Swift.
2. File → Add Package Dependencies → Add Local... → apontar para
   `mobile-ios/MesadaCore` (path relativo, não URL de git).
3. Arrastar os arquivos de `mobile-ios/MesadaApp/` para o projeto (grupos
   Views/, ViewModels/, Networking/, Persistence/, mantendo a mesma
   estrutura de pastas).
4. Copiar as chaves de `Info.plist.reference.xml` para o Info.plist real do
   target.
5. `swift test` dentro de `mobile-ios/MesadaCore` já deve passar de
   primeira — é a mesma lógica validada no Android via `kotlinc`.

## Por que a lógica central deveria estar correta mesmo sem compilar aqui

`MesadaCore` é a tradução linha-a-linha de `mobile-android/core-logic`
(mesmos nomes de tipo, mesma regra de negócio, mesmos casos de teste) — a
lógica já foi validada com um compilador real do lado Kotlin. O risco que
resta do lado Swift é só de sintaxe/API do próprio Swift, não de lógica de
negócio.
