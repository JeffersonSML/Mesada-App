import Foundation

/// Fila de sincronização offline-first — mesma lógica de
/// com.mesadaapp.mobile.core.FilaSincronizacao (Android), sem depender de
/// UIKit, rede ou de um mecanismo de persistência específico.
public final class FilaSincronizacao {
    private static let maxTentativasEnvio = 5

    private var itens: [String: ExecucaoPendente] = [:]
    private var ordemDeInsercao: [String] = []

    public init() {}

    public var tamanho: Int { itens.count }

    public func enfileirar(_ execucao: ExecucaoPendente) {
        if itens[execucao.execucaoLocalId] == nil {
            ordemDeInsercao.append(execucao.execucaoLocalId)
        }
        itens[execucao.execucaoLocalId] = execucao
    }

    public func pendentes() -> [ExecucaoPendente] {
        ordemDeInsercao.compactMap { itens[$0] }.filter { $0.tentativasEnvio < Self.maxTentativasEnvio }
    }

    public func comFalhaPermanente() -> [ExecucaoPendente] {
        ordemDeInsercao.compactMap { itens[$0] }.filter { $0.tentativasEnvio >= Self.maxTentativasEnvio }
    }

    /// Próximo item a enviar: o mais antigo (FIFO) entre os que ainda podem tentar.
    public func proximoParaEnvio() -> ExecucaoPendente? {
        pendentes().min { $0.timestampLocal < $1.timestampLocal }
    }

    public func marcarComoEnviado(_ execucaoLocalId: String) {
        itens.removeValue(forKey: execucaoLocalId)
        ordemDeInsercao.removeAll { $0 == execucaoLocalId }
    }

    public func marcarFalhaDeEnvio(_ execucaoLocalId: String) {
        guard var atual = itens[execucaoLocalId] else { return }
        atual.tentativasEnvio += 1
        itens[execucaoLocalId] = atual
    }
}
