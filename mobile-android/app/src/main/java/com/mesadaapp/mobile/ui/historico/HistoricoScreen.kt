package com.mesadaapp.mobile.ui.historico

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
 * O backend ainda não expõe ciclos fechados/execuções concluídas para o
 * usuário Comum (docs/adendo-mobile.md#telas-principais — Histórico). Sem
 * dado real para mostrar, esta tela é só o placeholder — nunca deve exibir
 * números inventados como se fossem reais.
 */
@Composable
fun HistoricoScreen() {
    Scaffold(topBar = { TopAppBar(title = { Text("Histórico") }) }) { paddingInterno ->
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
                text = "Aqui você vai ver as tarefas de ciclos anteriores e um extrato simplificado da sua mesada.",
                style = MaterialTheme.typography.bodyMedium,
                modifier = Modifier.padding(top = 8.dp),
            )
        }
    }
}
