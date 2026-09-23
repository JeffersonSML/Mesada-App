import XCTest
@testable import MesadaCore

final class FilaSincronizacaoTests: XCTestCase {

    private func execucao(id: String, timestamp: Date, tentativas: Int = 0) -> ExecucaoPendente {
        ExecucaoPendente(
            execucaoLocalId: id,
            tarefaUsuarioId: "tarefa-1",
            status: .feito,
            percentualConclusao: 100,
            timestampLocal: timestamp,
            evidenciaPathLocal: nil,
            tentativasEnvio: tentativas
        )
    }

    func testFilaVaziaNaoTemProximoItem() {
        XCTAssertNil(FilaSincronizacao().proximoParaEnvio())
    }

    func testProximoParaEnvioRetornaOMaisAntigoPrimeiro() {
        let fila = FilaSincronizacao()
        fila.enfileirar(execucao(id: "b", timestamp: Date(timeIntervalSince1970: 200)))
        fila.enfileirar(execucao(id: "a", timestamp: Date(timeIntervalSince1970: 100)))

        XCTAssertEqual(fila.proximoParaEnvio()?.execucaoLocalId, "a")
    }

    func testMarcarComoEnviadoRemoveOItemDaFila() {
        let fila = FilaSincronizacao()
        fila.enfileirar(execucao(id: "a", timestamp: Date(timeIntervalSince1970: 100)))

        fila.marcarComoEnviado("a")

        XCTAssertEqual(fila.tamanho, 0)
        XCTAssertNil(fila.proximoParaEnvio())
    }

    func testMarcarFalhaDeEnvioIncrementaTentativasSemRemover() {
        let fila = FilaSincronizacao()
        fila.enfileirar(execucao(id: "a", timestamp: Date(timeIntervalSince1970: 100)))

        fila.marcarFalhaDeEnvio("a")

        XCTAssertEqual(fila.pendentes().first?.tentativasEnvio, 1)
    }

    func testItemComTentativasEsgotadasSomeDePendentes() {
        let fila = FilaSincronizacao()
        fila.enfileirar(execucao(id: "a", timestamp: Date(timeIntervalSince1970: 100), tentativas: 5))

        XCTAssertTrue(fila.pendentes().isEmpty)
        XCTAssertEqual(fila.comFalhaPermanente().first?.execucaoLocalId, "a")
        XCTAssertNil(fila.proximoParaEnvio())
    }
}
