import SwiftUI

/// O backend ainda não expõe ciclos fechados/execuções concluídas para o
/// usuário Comum (docs/adendo-mobile.md#telas-principais — Histórico).
/// Mesma limitação de mobile-android/.../ui/historico/HistoricoScreen.kt.
struct HistoricoView: View {
    var body: some View {
        VStack(spacing: 8) {
            Text("Em breve").font(.title2).bold()
            Text("Aqui você vai ver as tarefas de ciclos anteriores e um extrato simplificado da sua mesada.")
                .foregroundStyle(.secondary)
                .multilineTextAlignment(.center)
        }
        .padding(24)
        .navigationTitle("Histórico")
    }
}
