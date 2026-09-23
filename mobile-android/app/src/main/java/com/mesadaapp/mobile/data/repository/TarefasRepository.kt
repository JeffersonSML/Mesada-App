package com.mesadaapp.mobile.data.repository

import com.mesadaapp.mobile.core.ExecucaoPendente
import com.mesadaapp.mobile.core.FilaSincronizacao
import com.mesadaapp.mobile.core.StatusExecucaoLocal
import com.mesadaapp.mobile.data.local.ExecucaoPendenteDao
import com.mesadaapp.mobile.data.local.ExecucaoPendenteEntity
import com.mesadaapp.mobile.data.local.TarefaDao
import com.mesadaapp.mobile.data.local.TarefaEntity
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

/**
 * O backend ainda não expõe um endpoint para listar tarefas/execuções do
 * usuário Comum (só existe hoje o resgate de convite — ver ApiService). Este
 * repositório já implementa de verdade a leitura/gravação local (Room) e a
 * fila de sincronização (:core-logic), para que `sincronizarPendentes()`
 * vire uma chamada HTTP real assim que o endpoint existir, sem precisar
 * redesenhar a camada offline. Até lá, `carregarExemplo()` popula o cache
 * local com dados claramente ilustrativos para as telas terem o que exibir
 * durante o desenvolvimento da UI — nunca chamado em build de produção.
 */
class TarefasRepository(
    private val tarefaDao: TarefaDao,
    private val execucaoPendenteDao: ExecucaoPendenteDao,
) {
    private val filaSincronizacao = FilaSincronizacao()

    fun observarTarefas(): Flow<List<TarefaEntity>> = tarefaDao.observarTarefas()

    suspend fun marcarConclusao(
        tarefaUsuarioId: String,
        status: StatusExecucaoLocal,
        percentualConclusao: Double,
        evidenciaPathLocal: String?,
    ) {
        val execucao = ExecucaoPendente(
            execucaoLocalId = java.util.UUID.randomUUID().toString(),
            tarefaUsuarioId = tarefaUsuarioId,
            status = status,
            percentualConclusao = percentualConclusao,
            timestampLocalEpochMillis = System.currentTimeMillis(),
            evidenciaPathLocal = evidenciaPathLocal,
        )
        filaSincronizacao.enfileirar(execucao)
        execucaoPendenteDao.enfileirar(execucao.paraEntity())
    }

    /**
     * Ponto de extensão: quando o backend expuser o endpoint de execuções,
     * iterar `filaSincronizacao.proximoParaEnvio()`, enviar via ApiService,
     * e chamar `marcarComoEnviado`/`marcarFalhaDeEnvio` conforme o
     * resultado — a decisão de qual é o próximo item e quando desistir já
     * está pronta e testada em :core-logic.
     */
    suspend fun sincronizarPendentes() {
        // TODO(Etapa backend seguinte): implementar quando existir o endpoint de execuções.
    }

    /** Dados de exemplo para desenvolvimento da UI — nunca usado em produção. */
    suspend fun carregarExemplo() {
        tarefaDao.salvarTarefas(
            listOf(
                TarefaEntity(
                    tarefaUsuarioId = "exemplo-1",
                    nome = "Arrumar o quarto",
                    descricao = "Cama feita, roupas no cesto, mesa organizada.",
                    prazoEpochMillis = null,
                    permiteParcial = true,
                ),
                TarefaEntity(
                    tarefaUsuarioId = "exemplo-2",
                    nome = "Treino de natação",
                    descricao = "Integração automática via Strava (Premium).",
                    prazoEpochMillis = null,
                    permiteParcial = false,
                ),
            )
        )
    }
}

private fun ExecucaoPendente.paraEntity() = ExecucaoPendenteEntity(
    execucaoLocalId = execucaoLocalId,
    tarefaUsuarioId = tarefaUsuarioId,
    status = status.name,
    percentualConclusao = percentualConclusao,
    timestampLocalEpochMillis = timestampLocalEpochMillis,
    evidenciaPathLocal = evidenciaPathLocal,
    tentativasEnvio = tentativasEnvio,
)
