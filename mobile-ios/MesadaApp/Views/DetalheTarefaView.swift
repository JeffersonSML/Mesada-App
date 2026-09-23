import SwiftUI
import MesadaCore

struct DetalheTarefaView: View {
    let tarefaUsuarioId: String
    @ObservedObject var tarefasRepository: TarefasRepository

    @State private var mostrarCamera = false
    @State private var evidenciaCapturada: UIImage?
    @Environment(\.dismiss) private var dismiss

    var body: some View {
        VStack(alignment: .leading, spacing: 16) {
            Text("Registre a evidência (foto) e marque como concluída.")
                .foregroundStyle(.secondary)

            if let evidencia = evidenciaCapturada {
                Image(uiImage: evidencia)
                    .resizable()
                    .scaledToFit()
                    .frame(maxHeight: 200)
                    .clipShape(RoundedRectangle(cornerRadius: 12))
            }

            Button(evidenciaCapturada == nil ? "Tirar foto" : "Foto registrada — tirar outra") {
                mostrarCamera = true
            }
            .buttonStyle(.bordered)

            Button("Concluir tarefa") {
                concluirTarefa()
            }
            .buttonStyle(.borderedProminent)
            .disabled(evidenciaCapturada == nil)

            Spacer()
        }
        .padding(24)
        .navigationTitle("Detalhe da tarefa")
        .sheet(isPresented: $mostrarCamera) {
            CameraPicker { imagem in evidenciaCapturada = imagem }
        }
    }

    private func concluirTarefa() {
        let caminhoLocal = salvarEvidenciaLocalmente(evidenciaCapturada)
        tarefasRepository.marcarConclusao(
            tarefaUsuarioId: tarefaUsuarioId,
            status: .feito,
            percentualConclusao: 100,
            evidenciaPathLocal: caminhoLocal
        )
        dismiss()
    }

    private func salvarEvidenciaLocalmente(_ imagem: UIImage?) -> String? {
        guard let imagem, let dados = imagem.jpegData(compressionQuality: 0.9) else { return nil }
        let diretorio = FileManager.default.urls(for: .cachesDirectory, in: .userDomainMask)[0]
        let arquivo = diretorio.appendingPathComponent("evidencia_\(UUID().uuidString).jpg")
        try? dados.write(to: arquivo)
        return arquivo.path
    }
}
