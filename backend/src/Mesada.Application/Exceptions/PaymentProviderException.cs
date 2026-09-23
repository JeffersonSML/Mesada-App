namespace Mesada.Application.Exceptions;

/// <summary>Erro retornado pelo gateway de pagamento (ex.: Pagar.me) — nunca deixa vazar o tipo/SDK concreto para fora de Infrastructure.</summary>
public sealed class PaymentProviderException(string mensagem, Exception? innerException = null)
    : Exception(mensagem, innerException);
