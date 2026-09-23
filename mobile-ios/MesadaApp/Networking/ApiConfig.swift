import Foundation

/// Configuração da API — nunca hardcoded em outro lugar do app.
/// `http://127.0.0.1:5080` funciona no Simulador iOS (que compartilha a
/// rede do Mac host, ao contrário do emulador Android). Em builds de
/// dispositivo físico/produção, sobrescrever via variável de ambiente de
/// build (xcconfig) apontando para um host real.
enum ApiConfig {
    static let baseURL = URL(string: ProcessInfo.processInfo.environment["API_BASE_URL"] ?? "http://127.0.0.1:5080")!
}
