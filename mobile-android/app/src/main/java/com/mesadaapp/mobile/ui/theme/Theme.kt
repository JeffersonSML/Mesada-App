package com.mesadaapp.mobile.ui.theme

import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

// Paleta simples, acolhedora — o usuário aqui é a criança/adolescente, não
// o adulto responsável (esse é o público do Painel Web, ver web/README.md).
private val AzulPrimario = Color(0xFF3B82F6)
private val VerdeSucesso = Color(0xFF22C55E)
private val VermelhoAlerta = Color(0xFFEF4444)

private val EsquemaClaro = lightColorScheme(
    primary = AzulPrimario,
    secondary = VerdeSucesso,
    error = VermelhoAlerta,
)

private val EsquemaEscuro = darkColorScheme(
    primary = AzulPrimario,
    secondary = VerdeSucesso,
    error = VermelhoAlerta,
)

@Composable
fun MesadaAppTheme(content: @Composable () -> Unit) {
    val esquemaDeCores = if (isSystemInDarkTheme()) EsquemaEscuro else EsquemaClaro
    MaterialTheme(colorScheme = esquemaDeCores, content = content)
}
