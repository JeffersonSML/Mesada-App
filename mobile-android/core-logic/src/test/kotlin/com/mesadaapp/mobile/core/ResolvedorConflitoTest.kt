package com.mesadaapp.mobile.core

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class ResolvedorConflitoTest {

    @Test
    fun `local mais recente que o remoto vence sem notificar o Master`() {
        val resultado = ResolvedorConflito.resolver(timestampLocal = 200, timestampRemoto = 100)

        assertEquals(VencedorConflito.LOCAL, resultado.vencedor)
        assertFalse(resultado.requerNotificacaoAoMaster)
    }

    @Test
    fun `remoto mais recente que o local vence e notifica o Master`() {
        val resultado = ResolvedorConflito.resolver(timestampLocal = 100, timestampRemoto = 200)

        assertEquals(VencedorConflito.REMOTO, resultado.vencedor)
        assertTrue(resultado.requerNotificacaoAoMaster)
    }

    @Test
    fun `empate e tratado como vitoria do remoto (fonte de verdade do Master)`() {
        val resultado = ResolvedorConflito.resolver(timestampLocal = 150, timestampRemoto = 150)

        assertEquals(VencedorConflito.REMOTO, resultado.vencedor)
        assertTrue(resultado.requerNotificacaoAoMaster)
    }
}
