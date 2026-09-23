import Foundation

/// Espelha o enum status_execucao do backend (infra/db/migrations/012_execucoes.sql)
/// e com.mesadaapp.mobile.core.StatusExecucaoLocal (Android).
public enum StatusExecucaoLocal: String, Codable, Sendable {
    case pendente
    case feito
    case parcial
    case naoFeito
}

/// Cópia local (offline) de uma Tarefa atribuída ao filho.
public struct TarefaLocal: Codable, Identifiable, Sendable {
    public var id: String { tarefaUsuarioId }
    public let tarefaUsuarioId: String
    public let nome: String
    public let descricao: String?
    public let prazo: Date?
    public let permiteParcial: Bool

    public init(tarefaUsuarioId: String, nome: String, descricao: String?, prazo: Date?, permiteParcial: Bool) {
        self.tarefaUsuarioId = tarefaUsuarioId
        self.nome = nome
        self.descricao = descricao
        self.prazo = prazo
        self.permiteParcial = permiteParcial
    }
}

/// Uma marcação de conclusão feita offline, aguardando envio ao backend.
public struct ExecucaoPendente: Codable, Identifiable, Sendable {
    public var id: String { execucaoLocalId }
    public let execucaoLocalId: String
    public let tarefaUsuarioId: String
    public let status: StatusExecucaoLocal
    public let percentualConclusao: Double
    public let timestampLocal: Date
    public let evidenciaPathLocal: String?
    public var tentativasEnvio: Int

    public init(
        execucaoLocalId: String = UUID().uuidString,
        tarefaUsuarioId: String,
        status: StatusExecucaoLocal,
        percentualConclusao: Double,
        timestampLocal: Date = Date(),
        evidenciaPathLocal: String?,
        tentativasEnvio: Int = 0
    ) {
        self.execucaoLocalId = execucaoLocalId
        self.tarefaUsuarioId = tarefaUsuarioId
        self.status = status
        self.percentualConclusao = percentualConclusao
        self.timestampLocal = timestampLocal
        self.evidenciaPathLocal = evidenciaPathLocal
        self.tentativasEnvio = tentativasEnvio
    }
}
