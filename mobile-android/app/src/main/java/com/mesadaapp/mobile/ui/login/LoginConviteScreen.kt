package com.mesadaapp.mobile.ui.login

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.mesadaapp.mobile.data.repository.AutenticacaoRepository

@Composable
fun LoginConviteScreen(
    autenticacaoRepository: AutenticacaoRepository,
    aoAutenticar: () -> Unit,
    viewModel: LoginConviteViewModel = viewModel(
        factory = LoginConviteViewModel.factory(autenticacaoRepository),
    ),
) {
    val estado by viewModel.estado.collectAsState()

    Scaffold { paddingInterno ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingInterno)
                .padding(24.dp),
            verticalArrangement = Arrangement.Center,
        ) {
            Text(
                text = "Entrar com código de convite",
                style = MaterialTheme.typography.headlineSmall,
            )
            Text(
                text = "Peça o código para o responsável pela sua família — ele foi gerado na Web.",
                style = MaterialTheme.typography.bodyMedium,
                modifier = Modifier.padding(top = 8.dp, bottom = 24.dp),
            )

            OutlinedTextField(
                value = estado.codigo,
                onValueChange = viewModel::aoMudarCodigo,
                label = { Text("Código do convite") },
                singleLine = true,
                modifier = Modifier.fillMaxWidth(),
            )

            if (estado.mensagemDeErro != null) {
                Text(
                    text = estado.mensagemDeErro!!,
                    color = MaterialTheme.colorScheme.error,
                    modifier = Modifier.padding(top = 8.dp),
                )
            }

            Button(
                onClick = { viewModel.resgatarConvite(aoSucesso = aoAutenticar) },
                enabled = !estado.carregando && estado.codigo.isNotBlank(),
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(top = 16.dp),
            ) {
                if (estado.carregando) {
                    CircularProgressIndicator(
                        modifier = Modifier.size(20.dp),
                        color = MaterialTheme.colorScheme.onPrimary,
                    )
                } else {
                    Text("Entrar")
                }
            }
        }
    }
}
