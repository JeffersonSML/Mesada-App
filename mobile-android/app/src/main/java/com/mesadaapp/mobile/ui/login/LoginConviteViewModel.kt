package com.mesadaapp.mobile.ui.login

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import com.mesadaapp.mobile.data.repository.AutenticacaoRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

data class LoginConviteEstado(
    val codigo: String = "",
    val carregando: Boolean = false,
    val mensagemDeErro: String? = null,
)

class LoginConviteViewModel(
    private val autenticacaoRepository: AutenticacaoRepository,
) : ViewModel() {

    private val _estado = MutableStateFlow(LoginConviteEstado())
    val estado: StateFlow<LoginConviteEstado> = _estado.asStateFlow()

    fun aoMudarCodigo(novoValor: String) {
        _estado.update { it.copy(codigo = novoValor, mensagemDeErro = null) }
    }

    fun resgatarConvite(aoSucesso: () -> Unit) {
        val codigo = _estado.value.codigo.trim()
        if (codigo.isBlank()) return

        _estado.update { it.copy(carregando = true, mensagemDeErro = null) }
        viewModelScope.launch {
            autenticacaoRepository.resgatarConvite(codigo)
                .onSuccess {
                    _estado.update { it.copy(carregando = false) }
                    aoSucesso()
                }
                .onFailure {
                    _estado.update {
                        it.copy(
                            carregando = false,
                            mensagemDeErro = "Código inválido ou expirado. Peça um novo convite ao responsável.",
                        )
                    }
                }
        }
    }

    companion object {
        fun factory(autenticacaoRepository: AutenticacaoRepository) = object : ViewModelProvider.Factory {
            @Suppress("UNCHECKED_CAST")
            override fun <T : ViewModel> create(modelClass: Class<T>): T =
                LoginConviteViewModel(autenticacaoRepository) as T
        }
    }
}
