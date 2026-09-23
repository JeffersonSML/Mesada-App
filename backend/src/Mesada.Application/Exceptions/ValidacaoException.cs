namespace Mesada.Application.Exceptions;

/// <summary>Violação de regra de negócio validável na camada de Application — sempre mapeada para 400 na Api.</summary>
public sealed class ValidacaoException(string mensagem) : Exception(mensagem);
