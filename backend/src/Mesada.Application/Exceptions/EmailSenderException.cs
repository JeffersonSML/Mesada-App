namespace Mesada.Application.Exceptions;

/// <summary>Erro retornado pelo provedor de e-mail (ex.: Resend) — nunca deixa vazar o tipo/SDK concreto para fora de Infrastructure.</summary>
public sealed class EmailSenderException(string mensagem, Exception? innerException = null)
    : Exception(mensagem, innerException);
