import Foundation

@MainActor
final class LoginConviteViewModel: ObservableObject {
    @Published var codigo: String = ""
    @Published private(set) var carregando = false
    @Published private(set) var mensagemDeErro: String?

    private let autenticacaoRepository: AutenticacaoRepository

    init(autenticacaoRepository: AutenticacaoRepository) {
        self.autenticacaoRepository = autenticacaoRepository
    }

    func resgatarConvite(aoSucesso: @escaping () -> Void) {
        let codigoLimpo = codigo.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !codigoLimpo.isEmpty else { return }

        carregando = true
        mensagemDeErro = nil

        Task {
            let resultado = await autenticacaoRepository.resgatarConvite(codigo: codigoLimpo)
            carregando = false
            switch resultado {
            case .success:
                aoSucesso()
            case .failure:
                mensagemDeErro = "Código inválido ou expirado. Peça um novo convite ao responsável."
            }
        }
    }
}
