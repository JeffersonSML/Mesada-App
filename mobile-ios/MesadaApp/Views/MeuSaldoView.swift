import SwiftUI

/// O backend ainda não expõe o ciclo de mesada em aberto para o usuário
/// Comum (docs/adendo-mobile.md#telas-principais — Meu Saldo). Mesma
/// limitação de mobile-android/.../ui/saldo/MeuSaldoScreen.kt.
struct MeuSaldoView: View {
    var body: some View {
        VStack(spacing: 8) {
            Text("Em breve").font(.title2).bold()
            Text("Aqui você vai ver o valor do ciclo atual, seu saldo devedor (se houver) e a próxima data de fechamento.")
                .foregroundStyle(.secondary)
                .multilineTextAlignment(.center)
        }
        .padding(24)
        .navigationTitle("Meu saldo")
    }
}
