package com.mesadaapp.mobile.data.remote

import com.mesadaapp.mobile.data.remote.dto.ResgatarConviteRequest
import com.mesadaapp.mobile.data.remote.dto.TokenResponse
import retrofit2.http.Body
import retrofit2.http.POST
import retrofit2.http.Path

/**
 * Único endpoint real hoje para o app do filho (docs/especificacao.md
 * #fluxo-de-convite). Não existe ainda no backend um endpoint para listar
 * tarefas/execuções do usuário Comum — quando existir, entra aqui seguindo
 * o mesmo padrão, e as telas em ui/tarefas, ui/historico e ui/saldo deixam
 * de usar dados de exemplo.
 */
interface ApiService {

    @POST("api/auth/convites/{codigo}/resgatar")
    suspend fun resgatarConvite(
        @Path("codigo") codigo: String,
        @Body request: ResgatarConviteRequest,
    ): TokenResponse
}
