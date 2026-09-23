package com.mesadaapp.mobile.data.repository

import com.mesadaapp.mobile.data.TokenStore
import com.mesadaapp.mobile.data.remote.ApiService
import com.mesadaapp.mobile.data.remote.dto.ResgatarConviteRequest

class AutenticacaoRepository(
    private val apiService: ApiService,
    private val tokenStore: TokenStore,
) {
    /**
     * Resgata o convite gerado pelo Master na Web e vincula este aparelho
     * (docs/especificacao.md#fluxo-de-convite). Em caso de sucesso, o JWT
     * fica salvo e as chamadas seguintes já saem autenticadas.
     */
    suspend fun resgatarConvite(codigo: String): Result<Unit> = runCatching {
        val dispositivoId = tokenStore.obterOuCriarDispositivoId()
        val resposta = apiService.resgatarConvite(codigo, ResgatarConviteRequest(dispositivoId))
        tokenStore.salvarToken(resposta.token)
    }

    suspend fun logout() = tokenStore.limparSessao()
}
