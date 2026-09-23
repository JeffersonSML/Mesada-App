package com.mesadaapp.mobile.data.local

import androidx.room.Dao
import androidx.room.Delete
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import kotlinx.coroutines.flow.Flow

@Dao
interface TarefaDao {
    @Query("SELECT * FROM tarefas_locais ORDER BY nome")
    fun observarTarefas(): Flow<List<TarefaEntity>>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun salvarTarefas(tarefas: List<TarefaEntity>)
}

@Dao
interface ExecucaoPendenteDao {
    @Query("SELECT * FROM execucoes_pendentes ORDER BY timestampLocalEpochMillis")
    suspend fun listarPendentes(): List<ExecucaoPendenteEntity>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun enfileirar(execucao: ExecucaoPendenteEntity)

    @Delete
    suspend fun remover(execucao: ExecucaoPendenteEntity)
}
