namespace Mesada.Application.Exceptions;

/// <summary>Ação restrita ao grupo Owner do painel administrativo (ou a uma permissão específica que o grupo do Administrador autenticado não tem) — sempre mapeada para 403 na Api.</summary>
public sealed class PermissaoNegadaException(string mensagem) : Exception(mensagem);
