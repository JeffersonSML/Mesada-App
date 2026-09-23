import Foundation
import Security

/// Guarda o JWT e o identificador do dispositivo vinculado no Keychain
/// (docs/adendo-mobile.md#autenticação-no-app) — nunca em UserDefaults, já
/// que é um token de sessão. Nunca guarda e-mail/senha: o filho não tem
/// essas credenciais, o acesso é só por convite.
final class TokenStore {
    private let servico = "com.mesadaapp.mobile.sessao"
    private let chaveToken = "jwt_token"
    private let chaveDispositivo = "dispositivo_id"

    var token: String? {
        get { ler(chave: chaveToken) }
        set {
            if let novoValor = newValue {
                salvar(chave: chaveToken, valor: novoValor)
            } else {
                remover(chave: chaveToken)
            }
        }
    }

    func obterOuCriarDispositivoId() -> String {
        if let existente = ler(chave: chaveDispositivo) {
            return existente
        }
        let novoId = UUID().uuidString
        salvar(chave: chaveDispositivo, valor: novoId)
        return novoId
    }

    func limparSessao() {
        remover(chave: chaveToken)
    }

    // MARK: - Keychain (implementação mínima, só o necessário)

    private func salvar(chave: String, valor: String) {
        let dados = Data(valor.utf8)
        let consulta: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: servico,
            kSecAttrAccount as String: chave,
        ]
        SecItemDelete(consulta as CFDictionary)

        var novoItem = consulta
        novoItem[kSecValueData as String] = dados
        SecItemAdd(novoItem as CFDictionary, nil)
    }

    private func ler(chave: String) -> String? {
        let consulta: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: servico,
            kSecAttrAccount as String: chave,
            kSecReturnData as String: true,
            kSecMatchLimit as String: kSecMatchLimitOne,
        ]
        var resultado: AnyObject?
        let status = SecItemCopyMatching(consulta as CFDictionary, &resultado)
        guard status == errSecSuccess, let dados = resultado as? Data else { return nil }
        return String(data: dados, encoding: .utf8)
    }

    private func remover(chave: String) {
        let consulta: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: servico,
            kSecAttrAccount as String: chave,
        ]
        SecItemDelete(consulta as CFDictionary)
    }
}
