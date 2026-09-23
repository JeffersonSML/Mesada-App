namespace Mesada.Application.Exceptions;

/// <summary>Erro retornado pelo provedor de push (ex.: FCM) — nunca deixa vazar o tipo/SDK concreto para fora de Infrastructure.</summary>
public sealed class PushNotificationException(string mensagem, Exception? innerException = null)
    : Exception(mensagem, innerException);
