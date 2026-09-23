namespace Mesada.Application.Exceptions;

public sealed class EmailJaCadastradoException(string email) : Exception($"Já existe uma conta cadastrada com o e-mail '{email}'.");
