package com.mesadaapp.mobile

import android.os.Bundle
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.Surface
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.fragment.app.FragmentActivity
import com.mesadaapp.mobile.ui.biometria.BloqueioScreen
import com.mesadaapp.mobile.ui.navigation.MesadaNavHost
import com.mesadaapp.mobile.ui.navigation.Rotas
import com.mesadaapp.mobile.ui.theme.MesadaAppTheme
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.runBlocking

/**
 * FragmentActivity (não ComponentActivity) porque BiometricPrompt exige um
 * host FragmentActivity — Compose's setContent funciona normalmente aqui,
 * já que FragmentActivity é uma ComponentActivity.
 */
class MainActivity : FragmentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val app = application as MesadaApplication

        setContent {
            MesadaAppTheme {
                Surface(modifier = Modifier.fillMaxSize()) {
                    // Leitura síncrona de uma única chave do DataStore no
                    // início da Activity — mesmo trade-off já aceito em
                    // data/remote/NetworkModule.kt (AuthInterceptor).
                    val temSessaoSalva = remember { runBlocking { app.tokenStore.tokenFlow.first() } != null }
                    var desbloqueado by remember { mutableStateOf(!temSessaoSalva) }

                    if (temSessaoSalva && !desbloqueado) {
                        BloqueioScreen(activity = this, aoDesbloquear = { desbloqueado = true })
                    } else {
                        MesadaNavHost(
                            app = app,
                            rotaInicial = if (temSessaoSalva) Rotas.MINHAS_TAREFAS else Rotas.LOGIN,
                        )
                    }
                }
            }
        }
    }
}
