package com.mesadaapp.mobile.ui.biometria

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.fragment.app.FragmentActivity

@Composable
fun BloqueioScreen(activity: FragmentActivity, aoDesbloquear: () -> Unit) {
    Scaffold { paddingInterno ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingInterno)
                .padding(24.dp),
            verticalArrangement = Arrangement.Center,
            horizontalAlignment = Alignment.CenterHorizontally,
        ) {
            Text(text = "Sessão bloqueada", style = MaterialTheme.typography.headlineSmall)
            Text(
                text = "Use sua biometria ou o PIN do aparelho para continuar.",
                style = MaterialTheme.typography.bodyMedium,
                modifier = Modifier.padding(top = 8.dp, bottom = 24.dp),
            )
            Button(onClick = { BiometricGate.autenticar(activity, aoSucesso = aoDesbloquear, aoFalhar = {}) }) {
                Text("Desbloquear")
            }
        }
    }

    // Já abre o prompt automaticamente ao entrar na tela — o botão acima é
    // só para tentar de novo se a pessoa cancelar o prompt do sistema.
    LaunchedEffect(Unit) {
        if (BiometricGate.estaDisponivel(activity)) {
            BiometricGate.autenticar(activity, aoSucesso = aoDesbloquear, aoFalhar = {})
        }
    }
}
