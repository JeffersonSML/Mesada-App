package com.mesadaapp.mobile.data

import android.content.Context
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

private val Context.dataStore by preferencesDataStore(name = "mesada_sessao")

/**
 * Guarda o JWT e o identificador do dispositivo vinculado
 * (docs/adendo-mobile.md#autenticação-no-app). Nunca guarda e-mail/senha —
 * o filho não tem essas credenciais; o dispositivo é vinculado via convite.
 */
class TokenStore(private val context: Context) {

    private object Chaves {
        val TOKEN = stringPreferencesKey("jwt_token")
        val DISPOSITIVO_ID = stringPreferencesKey("dispositivo_id")
    }

    val tokenFlow: Flow<String?> = context.dataStore.data.map { it[Chaves.TOKEN] }

    suspend fun salvarToken(token: String) {
        context.dataStore.edit { it[Chaves.TOKEN] = token }
    }

    suspend fun limparSessao() {
        context.dataStore.edit { it.clear() }
    }

    suspend fun obterOuCriarDispositivoId(): String {
        var id: String? = null
        context.dataStore.edit { prefs ->
            id = prefs[Chaves.DISPOSITIVO_ID] ?: java.util.UUID.randomUUID().toString().also {
                prefs[Chaves.DISPOSITIVO_ID] = it
            }
        }
        return id!!
    }
}
