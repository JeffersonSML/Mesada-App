import SwiftUI

@main
struct MesadaAppApp: App {
    @StateObject private var ambiente: AppEnvironment
    @State private var autenticado: Bool

    init() {
        let ambienteInicial = AppEnvironment()
        _ambiente = StateObject(wrappedValue: ambienteInicial)
        _autenticado = State(initialValue: ambienteInicial.tokenStore.token != nil)
    }

    var body: some Scene {
        WindowGroup {
            Group {
                if autenticado {
                    MinhasTarefasView(
                        tarefasRepository: ambiente.tarefasRepository,
                        autenticado: $autenticado
                    )
                } else {
                    LoginConviteView(
                        viewModel: LoginConviteViewModel(autenticacaoRepository: ambiente.autenticacaoRepository),
                        autenticado: $autenticado
                    )
                }
            }
            .environmentObject(ambiente)
        }
    }
}
