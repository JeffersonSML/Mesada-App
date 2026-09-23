package com.mesadaapp.mobile.core

/**
 * Espelha o enum status_execucao do backend (infra/db/migrations/012_execucoes.sql).
 * Nomes iguais aos usados na API para evitar uma camada de tradução a mais.
 */
enum class StatusExecucaoLocal {
    PENDENTE,
    FEITO,
    PARCIAL,
    NAO_FEITO,
}

/**
 * Cópia local (offline) de uma Tarefa atribuída ao filho — o suficiente para
 * exibir a lista "Minhas Tarefas" e o detalhe sem depender de conexão
 * (docs/adendo-mobile.md#fluxo-offline-first-e-sincronização).
 */
data class TarefaLocal(
    val tarefaUsuarioId: String,
    val nome: String,
    val descricao: String?,
    val prazoEpochMillis: Long?,
    val permiteParcial: Boolean,
)

/**
 * Uma marcação de conclusão feita offline pelo filho, aguardando envio ao
 * backend. `timestampLocalEpochMillis` é o momento em que o filho marcou a
 * tarefa no aparelho — é ele que decide o resultado em caso de conflito.
 */
data class ExecucaoPendente(
    val execucaoLocalId: String,
    val tarefaUsuarioId: String,
    val status: StatusExecucaoLocal,
    val percentualConclusao: Double,
    val timestampLocalEpochMillis: Long,
    val evidenciaPathLocal: String?,
    val tentativasEnvio: Int = 0,
)
