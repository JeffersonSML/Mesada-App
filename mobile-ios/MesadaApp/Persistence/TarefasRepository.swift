import Foundation
import MesadaCore

/// Mesma limitação documentada em mobile-android/.../data/repository/TarefasRepository.kt:
/// o backend ainda não expõe um endpoint para listar tarefas/execuções do
/// usuário Comum. `sincronizarPendentes()` já está pronto para virar uma
/// chamada HTTP real assim que o endpoint existir; `carregarExemplo()` só
/// existe para a UI ter o que exibir durante o desenvolvimento.
final class TarefasRepository: ObservableObject {
    @Published private(set) var tarefas: [TarefaLocal] = []

    private let armazenamento: ArmazenamentoLocal
    private let filaSincronizacao = FilaSincronizacao()

    init(armazenamento: ArmazenamentoLocal) {
        self.armazenamento = armazenamento
        self.tarefas = armazenamento.carregarTarefas()
        armazenamento.carregarFila().forEach { filaSincronizacao.enfileirar($0) }
    }

    func marcarConclusao(tarefaUsuarioId: String, status: StatusExecucaoLocal, percentualConclusao: Double, evidenciaPathLocal: String?) {
        let execucao = ExecucaoPendente(
            tarefaUsuarioId: tarefaUsuarioId,
            status: status,
            percentualConclusao: percentualConclusao,
            evidenciaPathLocal: evidenciaPathLocal
        )
        filaSincronizacao.enfileirar(execucao)
        armazenamento.salvarFila(filaSincronizacao.pendentes() + filaSincronizacao.comFalhaPermanente())
    }

    /// Ponto de extensão: iterar `filaSincronizacao.proximoParaEnvio()` e
    /// enviar via ApiService assim que o endpoint de execuções existir.
    func sincronizarPendentes() async {
        // TODO(Etapa backend seguinte): implementar quando existir o endpoint de execuções.
    }

    /// Dados de exemplo para desenvolvimento da UI — nunca usado em produção.
    func carregarExemplo() {
        guard tarefas.isEmpty else { return }
        tarefas = [
            TarefaLocal(tarefaUsuarioId: "exemplo-1", nome: "Arrumar o quarto", descricao: "Cama feita, roupas no cesto, mesa organizada.", prazo: nil, permiteParcial: true),
            TarefaLocal(tarefaUsuarioId: "exemplo-2", nome: "Treino de natação", descricao: "Integração automática via Strava (Premium).", prazo: nil, permiteParcial: false),
        ]
        armazenamento.salvarTarefas(tarefas)
    }
}
