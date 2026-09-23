namespace Mesada.Application.Exceptions;

/// <summary>Recurso tenant-scoped não encontrado — id inexistente ou pertencente a outra família (RLS já filtra, então "não encontrado" cobre os dois casos sem distinção).</summary>
public sealed class RecursoNaoEncontradoException(string mensagem) : Exception(mensagem);
