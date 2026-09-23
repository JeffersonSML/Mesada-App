package com.mesadaapp.mobile.ui.detalhe

import android.net.Uri
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import com.mesadaapp.mobile.core.StatusExecucaoLocal
import com.mesadaapp.mobile.data.repository.TarefasRepository
import kotlinx.coroutines.launch
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import java.io.File

@Composable
fun DetalheTarefaScreen(
    tarefaUsuarioId: String,
    tarefasRepository: TarefasRepository,
    aoConcluir: () -> Unit,
    viewModel: DetalheTarefaViewModel = viewModel(
        factory = DetalheTarefaViewModel.factory(tarefasRepository),
    ),
) {
    val contexto = LocalContext.current
    var evidenciaUri by remember { mutableStateOf<Uri?>(null) }

    val lancadorCamera = rememberLauncherForActivityResult(ActivityResultContracts.TakePicturePreview()) { bitmap ->
        if (bitmap != null) {
            val arquivo = File(contexto.cacheDir, "evidencia_${System.currentTimeMillis()}.jpg")
            arquivo.outputStream().use { saida -> bitmap.compress(android.graphics.Bitmap.CompressFormat.JPEG, 90, saida) }
            evidenciaUri = Uri.fromFile(arquivo)
        }
    }

    Scaffold(
        topBar = { TopAppBar(title = { Text("Detalhe da tarefa") }) },
    ) { paddingInterno ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingInterno)
                .padding(24.dp),
        ) {
            Text(
                text = "Registre a evidência (foto) e marque como concluída.",
                style = MaterialTheme.typography.bodyMedium,
            )

            Button(
                onClick = { lancadorCamera.launch(null) },
                modifier = Modifier.padding(top = 16.dp),
            ) {
                Text(if (evidenciaUri == null) "Tirar foto" else "Foto registrada — tirar outra")
            }

            Button(
                onClick = {
                    viewModel.marcarConcluida(
                        tarefaUsuarioId = tarefaUsuarioId,
                        evidenciaPathLocal = evidenciaUri?.path,
                        aoConcluir = aoConcluir,
                    )
                },
                enabled = evidenciaUri != null,
                modifier = Modifier.padding(top = 16.dp),
            ) {
                Text("Concluir tarefa")
            }
        }
    }
}

class DetalheTarefaViewModel(private val tarefasRepository: TarefasRepository) : ViewModel() {

    fun marcarConcluida(tarefaUsuarioId: String, evidenciaPathLocal: String?, aoConcluir: () -> Unit) {
        viewModelScope.launch {
            tarefasRepository.marcarConclusao(
                tarefaUsuarioId = tarefaUsuarioId,
                status = StatusExecucaoLocal.FEITO,
                percentualConclusao = 100.0,
                evidenciaPathLocal = evidenciaPathLocal,
            )
            aoConcluir()
        }
    }

    companion object {
        fun factory(tarefasRepository: TarefasRepository) = object : ViewModelProvider.Factory {
            @Suppress("UNCHECKED_CAST")
            override fun <T : ViewModel> create(modelClass: Class<T>): T =
                DetalheTarefaViewModel(tarefasRepository) as T
        }
    }
}
