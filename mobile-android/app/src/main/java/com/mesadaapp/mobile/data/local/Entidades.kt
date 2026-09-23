package com.mesadaapp.mobile.data.local

import androidx.room.Entity
import androidx.room.PrimaryKey

/** Cópia local de uma tarefa atribuída ao filho — cache para uso offline. */
@Entity(tableName = "tarefas_locais")
data class TarefaEntity(
    @PrimaryKey val tarefaUsuarioId: String,
    val nome: String,
    val descricao: String?,
    val prazoEpochMillis: Long?,
    val permiteParcial: Boolean,
)

/**
 * Uma marcação de conclusão feita offline, aguardando envio. Persistência
 * da fila cujo comportamento é decidido por
 * com.mesadaapp.mobile.core.FilaSincronizacao (módulo :core-logic).
 */
@Entity(tableName = "execucoes_pendentes")
data class ExecucaoPendenteEntity(
    @PrimaryKey val execucaoLocalId: String,
    val tarefaUsuarioId: String,
    val status: String,
    val percentualConclusao: Double,
    val timestampLocalEpochMillis: Long,
    val evidenciaPathLocal: String?,
    val tentativasEnvio: Int,
)
