package com.mesadaapp.mobile.ui.tarefas

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import com.mesadaapp.mobile.data.local.TarefaEntity
import com.mesadaapp.mobile.data.repository.TarefasRepository
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch

class MinhasTarefasViewModel(private val tarefasRepository: TarefasRepository) : ViewModel() {

    val tarefas: StateFlow<List<TarefaEntity>> = tarefasRepository.observarTarefas()
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), emptyList())

    /** Ver TarefasRepository.carregarExemplo — remover assim que o backend expuser o endpoint real. */
    fun garantirDadosDeExemplo() {
        viewModelScope.launch {
            if (tarefas.value.isEmpty()) {
                tarefasRepository.carregarExemplo()
            }
        }
    }

    companion object {
        fun factory(tarefasRepository: TarefasRepository) = object : ViewModelProvider.Factory {
            @Suppress("UNCHECKED_CAST")
            override fun <T : ViewModel> create(modelClass: Class<T>): T =
                MinhasTarefasViewModel(tarefasRepository) as T
        }
    }
}
