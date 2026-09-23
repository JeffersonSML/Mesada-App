package com.mesadaapp.mobile

import android.app.Application
import com.mesadaapp.mobile.data.TokenStore
import com.mesadaapp.mobile.data.local.MesadaDatabase
import com.mesadaapp.mobile.data.remote.ApiService
import com.mesadaapp.mobile.data.remote.NetworkModule
import com.mesadaapp.mobile.data.repository.AutenticacaoRepository
import com.mesadaapp.mobile.data.repository.TarefasRepository

/**
 * Localizador de serviços simples e explícito — sem framework de injeção de
 * dependência. O app tem poucas dependências e um único usuário (o filho);
 * Hilt/Koin seriam complexidade sem benefício claro neste estágio.
 */
class MesadaApplication : Application() {

    lateinit var tokenStore: TokenStore
        private set
    lateinit var apiService: ApiService
        private set
    lateinit var autenticacaoRepository: AutenticacaoRepository
        private set
    lateinit var tarefasRepository: TarefasRepository
        private set

    override fun onCreate() {
        super.onCreate()

        tokenStore = TokenStore(this)
        apiService = NetworkModule.criarApiService(tokenStore)
        autenticacaoRepository = AutenticacaoRepository(apiService, tokenStore)

        val database = MesadaDatabase.obterInstancia(this)
        tarefasRepository = TarefasRepository(database.tarefaDao(), database.execucaoPendenteDao())
    }
}
