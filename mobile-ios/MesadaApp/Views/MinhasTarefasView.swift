import SwiftUI
import MesadaCore

struct MinhasTarefasView: View {
    @ObservedObject var tarefasRepository: TarefasRepository
    @Binding var autenticado: Bool

    var body: some View {
        NavigationStack {
            List(tarefasRepository.tarefas) { tarefa in
                NavigationLink(value: tarefa.tarefaUsuarioId) {
                    VStack(alignment: .leading, spacing: 4) {
                        Text(tarefa.nome).font(.headline)
                        if let descricao = tarefa.descricao {
                            Text(descricao).font(.subheadline).foregroundStyle(.secondary)
                        }
                    }
                }
            }
            .navigationTitle("Minhas tarefas")
            .navigationDestination(for: String.self) { tarefaUsuarioId in
                DetalheTarefaView(tarefaUsuarioId: tarefaUsuarioId, tarefasRepository: tarefasRepository)
            }
            .toolbar {
                ToolbarItem(placement: .navigationBarTrailing) {
                    NavigationLink("Histórico") { HistoricoView() }
                }
                ToolbarItem(placement: .navigationBarLeading) {
                    NavigationLink("Meu saldo") { MeuSaldoView() }
                }
            }
            .onAppear { tarefasRepository.carregarExemplo() }
        }
    }
}
