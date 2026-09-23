import Foundation

enum ApiError: Error {
    case respostaInvalida
    case http(status: Int)
}

/// Único endpoint real hoje para o app do filho (docs/especificacao.md
/// #fluxo-de-convite) — mesma limitação documentada em
/// mobile-android/app/.../data/remote/ApiService.kt: o backend ainda não
/// expõe listagem de tarefas/execuções para o usuário Comum.
final class ApiService {
    private let sessao: URLSession
    private let tokenStore: TokenStore

    init(tokenStore: TokenStore, sessao: URLSession = .shared) {
        self.tokenStore = tokenStore
        self.sessao = sessao
    }

    func resgatarConvite(codigo: String, dispositivoId: String) async throws -> TokenResponse {
        var url = ApiConfig.baseURL
        url.append(path: "/api/auth/convites/\(codigo)/resgatar")

        var requisicao = URLRequest(url: url)
        requisicao.httpMethod = "POST"
        requisicao.setValue("application/json", forHTTPHeaderField: "Content-Type")
        requisicao.httpBody = try JSONEncoder().encode(ResgatarConviteRequest(dispositivoId: dispositivoId))

        let (dados, resposta) = try await sessao.data(for: requisicao)
        guard let http = resposta as? HTTPURLResponse else { throw ApiError.respostaInvalida }
        guard (200..<300).contains(http.statusCode) else { throw ApiError.http(status: http.statusCode) }

        return try JSONDecoder().decode(TokenResponse.self, from: dados)
    }
}
