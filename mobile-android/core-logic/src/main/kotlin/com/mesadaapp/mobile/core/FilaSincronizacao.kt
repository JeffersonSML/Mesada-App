package com.mesadaapp.mobile.core

/**
 * Fila de sincronização offline-first (docs/adendo-mobile.md#fluxo-offline-
 * first-e-sincronização): toda conclusão de tarefa feita sem conexão entra
 * aqui e só sai quando confirmada pelo backend. Não depende de Android
 * (SQLite/Room) nem de rede — a persistência real e o envio HTTP são
 * responsabilidade das camadas de fora; esta classe só decide QUAL é o
 * próximo item a enviar e como reagir a sucesso/falha.
 *
 * Número máximo de tentativas antes de marcar um item como precisando de
 * intervenção manual (não fica tentando para sempre em loop silencioso).
 */
private const val MAX_TENTATIVAS_ENVIO = 5

class FilaSincronizacao {
    private val itens = LinkedHashMap<String, ExecucaoPendente>()

    val tamanho: Int
        get() = itens.size

    fun enfileirar(execucao: ExecucaoPendente) {
        itens[execucao.execucaoLocalId] = execucao
    }

    fun pendentes(): List<ExecucaoPendente> =
        itens.values.filter { it.tentativasEnvio < MAX_TENTATIVAS_ENVIO }

    fun comFalhaPermanente(): List<ExecucaoPendente> =
        itens.values.filter { it.tentativasEnvio >= MAX_TENTATIVAS_ENVIO }

    /** Próximo item a enviar: o mais antigo (FIFO) entre os que ainda podem tentar. */
    fun proximoParaEnvio(): ExecucaoPendente? =
        pendentes().minByOrNull { it.timestampLocalEpochMillis }

    fun marcarComoEnviado(execucaoLocalId: String) {
        itens.remove(execucaoLocalId)
    }

    fun marcarFalhaDeEnvio(execucaoLocalId: String) {
        val atual = itens[execucaoLocalId] ?: return
        itens[execucaoLocalId] = atual.copy(tentativasEnvio = atual.tentativasEnvio + 1)
    }
}
