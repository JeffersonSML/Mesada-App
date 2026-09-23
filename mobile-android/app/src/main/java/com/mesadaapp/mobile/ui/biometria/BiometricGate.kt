package com.mesadaapp.mobile.ui.biometria

import androidx.biometric.BiometricManager
import androidx.biometric.BiometricPrompt
import androidx.core.content.ContextCompat
import androidx.fragment.app.FragmentActivity

private const val AUTENTICADORES_PERMITIDOS =
    BiometricManager.Authenticators.BIOMETRIC_WEAK or BiometricManager.Authenticators.DEVICE_CREDENTIAL

/**
 * Bloqueio local após o vínculo inicial via convite — "PIN definido no app
 * ou biometria do aparelho" (docs/adendo-mobile.md#autenticação-no-app).
 * Usamos só o mecanismo de credencial do próprio Android (biometria OU o
 * PIN/padrão/senha que a pessoa já configurou no aparelho) em vez de
 * implementar um PIN próprio do app — menos código de segurança para
 * manter, e a credencial do aparelho já é tão forte quanto um PIN nosso.
 */
object BiometricGate {

    fun estaDisponivel(activity: FragmentActivity): Boolean {
        val gerenciador = BiometricManager.from(activity)
        return gerenciador.canAuthenticate(AUTENTICADORES_PERMITIDOS) == BiometricManager.BIOMETRIC_SUCCESS
    }

    fun autenticar(activity: FragmentActivity, aoSucesso: () -> Unit, aoFalhar: (String) -> Unit) {
        val executor = ContextCompat.getMainExecutor(activity)
        val prompt = BiometricPrompt(
            activity,
            executor,
            object : BiometricPrompt.AuthenticationCallback() {
                override fun onAuthenticationSucceeded(result: BiometricPrompt.AuthenticationResult) {
                    aoSucesso()
                }

                override fun onAuthenticationError(errorCode: Int, errString: CharSequence) {
                    aoFalhar(errString.toString())
                }

                override fun onAuthenticationFailed() {
                    // Tentativa não reconhecida (ex.: dedo errado) — o próprio
                    // prompt do sistema deixa a pessoa tentar de novo, não
                    // precisamos fazer nada aqui além de não avançar.
                }
            },
        )

        // setAllowedAuthenticators com DEVICE_CREDENTIAL não pode ser combinado
        // com setNegativeButtonText — o próprio PIN/padrão do aparelho já é a
        // opção de contingência quando a biometria falha ou não está configurada.
        val promptInfo = BiometricPrompt.PromptInfo.Builder()
            .setTitle("Desbloquear Mesada App")
            .setSubtitle("Use sua biometria ou o PIN do aparelho")
            .setAllowedAuthenticators(AUTENTICADORES_PERMITIDOS)
            .build()

        prompt.authenticate(promptInfo)
    }
}
