package com.mesadaapp.mobile.core

/**
 * "Conflitos (ex.: tarefa alterada na Web enquanto o app estava offline)
 * resolvidos por timestamp — a mudança mais recente prevalece, com o Master
 * notificado se um conflito de valor ocorrer." (docs/adendo-mobile.md)
 */
enum class VencedorConflito { LOCAL, REMOTO }

data class ResultadoConflito(
    val vencedor: VencedorConflito,
    /** true quando a versão local foi descartada — o Master precisa ser avisado. */
    val requerNotificacaoAoMaster: Boolean,
)

object ResolvedorConflito {

    /**
     * @param timestampLocal momento em que o filho marcou a execução no aparelho, offline.
     * @param timestampRemoto momento da última alteração dessa mesma execução no backend.
     *
     * Empate (timestamps iguais) é tratado como vitória do remoto: não há
     * como provar que a marcação local é estritamente mais recente, então o
     * lado servidor — que já é a fonte de verdade para o Master — prevalece,
     * e o filho é avisado de que precisa conferir/refazer.
     */
    fun resolver(timestampLocal: Long, timestampRemoto: Long): ResultadoConflito =
        if (timestampLocal > timestampRemoto) {
            ResultadoConflito(vencedor = VencedorConflito.LOCAL, requerNotificacaoAoMaster = false)
        } else {
            ResultadoConflito(vencedor = VencedorConflito.REMOTO, requerNotificacaoAoMaster = true)
        }
}
