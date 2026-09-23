import Foundation
import MesadaCore

/// Persistência local simples baseada em arquivo JSON — cumpre o mesmo
/// papel do Room no Android (mobile-android/.../data/local) sem precisar de
/// Core Data para um volume de dados pequeno (tarefas de um ciclo + fila de
/// sincronização de um único usuário).
final class ArmazenamentoLocal {
    private let diretorio: URL
    private let tarefasURL: URL
    private let filaURL: URL

    init(diretorio: URL = FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask)[0]) {
        self.diretorio = diretorio
        self.tarefasURL = diretorio.appendingPathComponent("tarefas_locais.json")
        self.filaURL = diretorio.appendingPathComponent("execucoes_pendentes.json")
        try? FileManager.default.createDirectory(at: diretorio, withIntermediateDirectories: true)
    }

    func carregarTarefas() -> [TarefaLocal] {
        carregar(de: tarefasURL) ?? []
    }

    func salvarTarefas(_ tarefas: [TarefaLocal]) {
        salvar(tarefas, em: tarefasURL)
    }

    func carregarFila() -> [ExecucaoPendente] {
        carregar(de: filaURL) ?? []
    }

    func salvarFila(_ execucoes: [ExecucaoPendente]) {
        salvar(execucoes, em: filaURL)
    }

    private func carregar<T: Decodable>(de url: URL) -> T? {
        guard let dados = try? Data(contentsOf: url) else { return nil }
        return try? JSONDecoder().decode(T.self, from: dados)
    }

    private func salvar<T: Encodable>(_ valor: T, em url: URL) {
        guard let dados = try? JSONEncoder().encode(valor) else { return }
        try? dados.write(to: url, options: .atomic)
    }
}
