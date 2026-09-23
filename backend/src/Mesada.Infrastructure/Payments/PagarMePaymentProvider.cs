using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using PagarMe;
using PagarMe.Models.Request;
using PagarMe.Models.Response;

namespace Mesada.Infrastructure.Payments;

/// <summary>
/// Implementação concreta de IPaymentProvider sobre o SDK oficial
/// "PagarMe" (NuGet), API v5 da Stone/Pagar.me. O SDK é síncrono — cada
/// chamada roda em Task.Run para não bloquear o contexto assíncrono do
/// chamador. NÃO validado contra a API real do Pagar.me nesta sessão: não
/// havia uma chave de sandbox disponível (ver docs/adr/0006-*.md). Toda
/// assinatura/nome de tipo abaixo foi conferida por reflexão sobre o DLL
/// real do pacote antes de escrever este arquivo — não é código adivinhado.
/// </summary>
public sealed class PagarMePaymentProvider(IPagarMeApiClient client) : IPaymentProvider
{
    public async Task<string> CriarClienteAsync(Guid familiaId, string nome, string email, CancellationToken ct = default)
    {
        var request = new CreateCustomerRequest
        {
            Name = nome,
            Email = email,
            Code = familiaId.ToString(),
            Type = "individual",
        };

        var resposta = await Task.Run(() => client.Customer.CreateCustomer(request), ct);
        return GarantirSucesso(resposta).Id;
    }

    public async Task<string> CriarAssinaturaAsync(
        string providerCustomerId,
        string planoCodigo,
        int precoMensalCentavos,
        string cardToken,
        CancellationToken ct = default)
    {
        var request = new CreateSubscriptionRequest
        {
            CustomerId = providerCustomerId,
            CardToken = cardToken,
            PaymentMethod = "credit_card",
            Interval = "month",
            IntervalCount = 1,
            Items =
            [
                new CreateSubscriptionItemRequest
                {
                    Description = planoCodigo,
                    Quantity = 1,
                    PricingScheme = new CreatePricingSchemeRequest
                    {
                        SchemeType = "unit",
                        Price = precoMensalCentavos,
                    },
                },
            ],
        };

        var resposta = await Task.Run(() => client.Subscription.CreateSubscription(request), ct);
        return GarantirSucesso(resposta).Id;
    }

    public async Task CancelarAssinaturaAsync(string providerSubscriptionId, CancellationToken ct = default)
    {
        var request = new CreateCancelSubscriptionRequest { CancelPendingInvoices = true };
        var resposta = await Task.Run(() => client.Subscription.CancelSubscription(providerSubscriptionId, request), ct);
        GarantirSucesso(resposta);
    }

    private static TSuccess GarantirSucesso<TSuccess>(RestSharp.Easy.Models.BaseResponse<TSuccess, PagarMeErrorsResponse> resposta)
    {
        if (resposta.IsSuccessfully && resposta.Data is not null)
            return resposta.Data;

        var mensagem = resposta.Error?.Message ?? resposta.Exception?.Message ?? "Erro desconhecido retornado pelo Pagar.me.";
        throw new PaymentProviderException(mensagem, resposta.Exception);
    }
}
