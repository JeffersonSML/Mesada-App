package com.mesadaapp.mobile.ui.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.mesadaapp.mobile.MesadaApplication
import com.mesadaapp.mobile.ui.detalhe.DetalheTarefaScreen
import com.mesadaapp.mobile.ui.historico.HistoricoScreen
import com.mesadaapp.mobile.ui.login.LoginConviteScreen
import com.mesadaapp.mobile.ui.saldo.MeuSaldoScreen
import com.mesadaapp.mobile.ui.tarefas.MinhasTarefasScreen

object Rotas {
    const val LOGIN = "login"
    const val MINHAS_TAREFAS = "minhas_tarefas"
    const val DETALHE_TAREFA = "detalhe_tarefa/{tarefaUsuarioId}"
    const val HISTORICO = "historico"
    const val MEU_SALDO = "meu_saldo"

    fun detalheTarefa(tarefaUsuarioId: String) = "detalhe_tarefa/$tarefaUsuarioId"
}

@Composable
fun MesadaNavHost(
    app: MesadaApplication,
    rotaInicial: String = Rotas.LOGIN,
    navController: NavHostController = rememberNavController(),
) {
    NavHost(navController = navController, startDestination = rotaInicial) {
        composable(Rotas.LOGIN) {
            LoginConviteScreen(
                autenticacaoRepository = app.autenticacaoRepository,
                aoAutenticar = {
                    navController.navigate(Rotas.MINHAS_TAREFAS) {
                        popUpTo(Rotas.LOGIN) { inclusive = true }
                    }
                },
            )
        }
        composable(Rotas.MINHAS_TAREFAS) {
            MinhasTarefasScreen(
                tarefasRepository = app.tarefasRepository,
                aoAbrirTarefa = { id -> navController.navigate(Rotas.detalheTarefa(id)) },
                aoAbrirHistorico = { navController.navigate(Rotas.HISTORICO) },
                aoAbrirSaldo = { navController.navigate(Rotas.MEU_SALDO) },
            )
        }
        composable(Rotas.DETALHE_TAREFA) { entrada ->
            val tarefaUsuarioId = entrada.arguments?.getString("tarefaUsuarioId").orEmpty()
            DetalheTarefaScreen(
                tarefaUsuarioId = tarefaUsuarioId,
                tarefasRepository = app.tarefasRepository,
                aoConcluir = { navController.popBackStack() },
            )
        }
        composable(Rotas.HISTORICO) { HistoricoScreen() }
        composable(Rotas.MEU_SALDO) { MeuSaldoScreen() }
    }
}
