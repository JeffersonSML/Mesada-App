import Foundation

/// Localizador de serviços simples e explícito — mesmo raciocínio de
/// com.mesadaapp.mobile.MesadaApplication (Android): poucas dependências,
/// um único usuário, sem framework de DI.
final class AppEnvironment: ObservableObject {
    let tokenStore: TokenStore
    let apiService: ApiService
    let autenticacaoRepository: AutenticacaoRepository
    let tarefasRepository: TarefasRepository

    init() {
        let tokenStore = TokenStore()
        self.tokenStore = tokenStore
        self.apiService = ApiService(tokenStore: tokenStore)
        self.autenticacaoRepository = AutenticacaoRepository(apiService: apiService, tokenStore: tokenStore)
        self.tarefasRepository = TarefasRepository(armazenamento: ArmazenamentoLocal())
    }
}
