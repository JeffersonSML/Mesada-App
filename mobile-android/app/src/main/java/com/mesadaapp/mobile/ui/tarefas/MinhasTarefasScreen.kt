package com.mesadaapp.mobile.ui.tarefas

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.Card
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.mesadaapp.mobile.data.local.TarefaEntity
import com.mesadaapp.mobile.data.repository.TarefasRepository

@Composable
fun MinhasTarefasScreen(
    tarefasRepository: TarefasRepository,
    aoAbrirTarefa: (String) -> Unit,
    aoAbrirHistorico: () -> Unit,
    aoAbrirSaldo: () -> Unit,
    viewModel: MinhasTarefasViewModel = viewModel(
        factory = MinhasTarefasViewModel.factory(tarefasRepository),
    ),
) {
    val tarefas by viewModel.tarefas.collectAsState()

    LaunchedEffect(Unit) { viewModel.garantirDadosDeExemplo() }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Minhas tarefas") },
                navigationIcon = { TextButton(onClick = aoAbrirSaldo) { Text("Saldo") } },
                actions = { TextButton(onClick = aoAbrirHistorico) { Text("Histórico") } },
            )
        },
    ) { paddingInterno ->
        Column(modifier = Modifier.padding(paddingInterno)) {
            // Navegação simples via texto no topo — vira uma barra de
            // navegação inferior (bottom nav) quando Histórico/Saldo
            // tiverem dados reais (ver ui/historico e ui/saldo).
            LazyColumn(modifier = Modifier.fillMaxSize()) {
                items(tarefas, key = { it.tarefaUsuarioId }) { tarefa ->
                    CartaoTarefa(tarefa = tarefa, aoClicar = { aoAbrirTarefa(tarefa.tarefaUsuarioId) })
                }
            }
        }
    }
}

@Composable
private fun CartaoTarefa(tarefa: TarefaEntity, aoClicar: () -> Unit) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .padding(horizontal = 16.dp, vertical = 8.dp)
            .clickable(onClick = aoClicar),
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Text(text = tarefa.nome, style = MaterialTheme.typography.titleMedium)
            if (tarefa.descricao != null) {
                Text(text = tarefa.descricao, style = MaterialTheme.typography.bodyMedium)
            }
        }
    }
}
