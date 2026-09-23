import Foundation

/// "Conflitos (ex.: tarefa alterada na Web enquanto o app estava offline)
/// resolvidos por timestamp — a mudança mais recente prevalece, com o
/// Master notificado se um conflito de valor ocorrer."
/// (docs/adendo-mobile.md) — mesma regra de
/// com.mesadaapp.mobile.core.ResolvedorConflito (Android).
public enum VencedorConflito: Sendable {
    case local
    case remoto
}

public struct ResultadoConflito: Sendable {
    public let vencedor: VencedorConflito
    /// true quando a versão local foi descartada — o Master precisa ser avisado.
    public let requerNotificacaoAoMaster: Bool
}

public enum ResolvedorConflito {

    /// Empate é tratado como vitória do remoto: o servidor é a fonte de
    /// verdade do Master, então em caso de dúvida ele prevalece e o filho é
    /// avisado para conferir/refazer.
    public static func resolver(timestampLocal: Date, timestampRemoto: Date) -> ResultadoConflito {
        if timestampLocal > timestampRemoto {
            return ResultadoConflito(vencedor: .local, requerNotificacaoAoMaster: false)
        } else {
            return ResultadoConflito(vencedor: .remoto, requerNotificacaoAoMaster: true)
        }
    }
}
