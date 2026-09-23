import XCTest
@testable import MesadaCore

final class ResolvedorConflitoTests: XCTestCase {

    func testLocalMaisRecenteVenceSemNotificar() {
        let resultado = ResolvedorConflito.resolver(
            timestampLocal: Date(timeIntervalSince1970: 200),
            timestampRemoto: Date(timeIntervalSince1970: 100)
        )

        XCTAssertEqual(resultado.vencedor, .local)
        XCTAssertFalse(resultado.requerNotificacaoAoMaster)
    }

    func testRemotoMaisRecenteVenceENotifica() {
        let resultado = ResolvedorConflito.resolver(
            timestampLocal: Date(timeIntervalSince1970: 100),
            timestampRemoto: Date(timeIntervalSince1970: 200)
        )

        XCTAssertEqual(resultado.vencedor, .remoto)
        XCTAssertTrue(resultado.requerNotificacaoAoMaster)
    }

    func testEmpateETratadoComoVitoriaDoRemoto() {
        let mesmoInstante = Date(timeIntervalSince1970: 150)
        let resultado = ResolvedorConflito.resolver(timestampLocal: mesmoInstante, timestampRemoto: mesmoInstante)

        XCTAssertEqual(resultado.vencedor, .remoto)
        XCTAssertTrue(resultado.requerNotificacaoAoMaster)
    }
}
