import Foundation

final class AutenticacaoRepository {
    private let apiService: ApiService
    private let tokenStore: TokenStore

    init(apiService: ApiService, tokenStore: TokenStore) {
        self.apiService = apiService
        self.tokenStore = tokenStore
    }

    /// Resgata o convite gerado pelo Master na Web e vincula este aparelho
    /// (docs/especificacao.md#fluxo-de-convite).
    func resgatarConvite(codigo: String) async -> Result<Void, Error> {
        do {
            let dispositivoId = tokenStore.obterOuCriarDispositivoId()
            let resposta = try await apiService.resgatarConvite(codigo: codigo, dispositivoId: dispositivoId)
            tokenStore.token = resposta.token
            return .success(())
        } catch {
            return .failure(error)
        }
    }

    func logout() {
        tokenStore.limparSessao()
    }
}
