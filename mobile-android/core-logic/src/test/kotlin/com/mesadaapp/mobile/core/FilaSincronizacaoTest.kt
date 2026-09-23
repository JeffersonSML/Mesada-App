package com.mesadaapp.mobile.core

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertNull
import kotlin.test.assertTrue

class FilaSincronizacaoTest {

    private fun execucao(id: String, timestamp: Long, tentativas: Int = 0) = ExecucaoPendente(
        execucaoLocalId = id,
        tarefaUsuarioId = "tarefa-1",
        status = StatusExecucaoLocal.FEITO,
        percentualConclusao = 100.0,
        timestampLocalEpochMillis = timestamp,
        evidenciaPathLocal = null,
        tentativasEnvio = tentativas,
    )

    @Test
    fun `fila vazia nao tem proximo item`() {
        assertNull(FilaSincronizacao().proximoParaEnvio())
    }

    @Test
    fun `proximoParaEnvio retorna o mais antigo primeiro (FIFO)`() {
        val fila = FilaSincronizacao()
        fila.enfileirar(execucao("b", timestamp = 200))
        fila.enfileirar(execucao("a", timestamp = 100))

        assertEquals("a", fila.proximoParaEnvio()?.execucaoLocalId)
    }

    @Test
    fun `marcarComoEnviado remove o item da fila`() {
        val fila = FilaSincronizacao()
        fila.enfileirar(execucao("a", timestamp = 100))

        fila.marcarComoEnviado("a")

        assertEquals(0, fila.tamanho)
        assertNull(fila.proximoParaEnvio())
    }

    @Test
    fun `marcarFalhaDeEnvio incrementa tentativas sem remover o item`() {
        val fila = FilaSincronizacao()
        fila.enfileirar(execucao("a", timestamp = 100))

        fila.marcarFalhaDeEnvio("a")

        assertEquals(1, fila.pendentes().single().tentativasEnvio)
    }

    @Test
    fun `item com tentativas esgotadas some de pendentes e aparece em comFalhaPermanente`() {
        val fila = FilaSincronizacao()
        fila.enfileirar(execucao("a", timestamp = 100, tentativas = 5))

        assertTrue(fila.pendentes().isEmpty())
        assertEquals("a", fila.comFalhaPermanente().single().execucaoLocalId)
        assertNull(fila.proximoParaEnvio())
    }
}
