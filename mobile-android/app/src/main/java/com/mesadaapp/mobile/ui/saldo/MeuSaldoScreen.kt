package com.mesadaapp.mobile.ui.saldo

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

/**
 * O backend ainda não expõe o ciclo de mesada em aberto para o usuário
 * Comum (docs/adendo-mobile.md#telas-principais — Meu Saldo). Placeholder
 * até existir o endpoint — nunca inventar um saldo.
 */
@Composable
fun MeuSaldoScreen() {
    Scaffold(topBar = { TopAppBar(title = { Text("Meu saldo") }) }) { paddingInterno ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingInterno)
                .padding(24.dp),
            verticalArrangement = Arrangement.Center,
            horizontalAlignment = Alignment.CenterHorizontally,
        ) {
            Text(text = "Em breve", style = MaterialTheme.typography.headlineSmall)
            Text(
                text = "Aqui você vai ver o valor do ciclo atual, seu saldo devedor (se houver) e a próxima data de fechamento.",
                style = MaterialTheme.typography.bodyMedium,
                modifier = Modifier.padding(top = 8.dp),
            )
        }
    }
}
