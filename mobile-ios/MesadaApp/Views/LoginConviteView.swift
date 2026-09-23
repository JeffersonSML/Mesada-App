import SwiftUI

struct LoginConviteView: View {
    @StateObject var viewModel: LoginConviteViewModel
    @Binding var autenticado: Bool

    var body: some View {
        VStack(alignment: .leading, spacing: 16) {
            Text("Entrar com código de convite")
                .font(.title2).bold()
            Text("Peça o código para o responsável pela sua família — ele foi gerado na Web.")
                .font(.body)
                .foregroundStyle(.secondary)

            TextField("Código do convite", text: $viewModel.codigo)
                .textFieldStyle(.roundedBorder)
                .autocapitalization(.allCharacters)

            if let mensagem = viewModel.mensagemDeErro {
                Text(mensagem).foregroundStyle(.red)
            }

            Button {
                viewModel.resgatarConvite { autenticado = true }
            } label: {
                if viewModel.carregando {
                    ProgressView()
                } else {
                    Text("Entrar").frame(maxWidth: .infinity)
                }
            }
            .buttonStyle(.borderedProminent)
            .disabled(viewModel.carregando || viewModel.codigo.isEmpty)

            Spacer()
        }
        .padding(24)
    }
}
