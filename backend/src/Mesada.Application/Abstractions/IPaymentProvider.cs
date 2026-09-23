namespace Mesada.Application.Abstractions;

/// <summary>
/// Abstrai o gateway de pagamento (docs/especificacao.md
/// #planos-e-assinatura) — hoje Stone/Pagar.me (ver
/// Mesada.Infrastructure.Payments.PagarMePaymentProvider), mas nenhum
/// código de Application ou Api deve referenciar um SDK de gateway
/// diretamente, exatamente para permitir trocar ou adicionar um segundo
/// provedor (ex.: InfinitePay) sem tocar no resto do sistema.
/// </summary>
public interface IPaymentProvider
{
    Task<string> CriarClienteAsync(Guid familiaId, string nome, string email, CancellationToken ct = default);

    /// <summary>
    /// <paramref name="cardToken"/> é gerado no cliente (Web) via SDK
    /// JS do gateway — o número do cartão nunca trafega pelo nosso backend
    /// (fora do escopo PCI-DSS). <paramref name="precoMensalCentavos"/> vem
    /// de Plano.PrecoMensal, resolvido pela Application antes de chamar
    /// esta interface — o provedor de pagamento não conhece nossas tabelas.
    /// </summary>
    Task<string> CriarAssinaturaAsync(
        string providerCustomerId,
        string planoCodigo,
        int precoMensalCentavos,
        string cardToken,
        CancellationToken ct = default);

    Task CancelarAssinaturaAsync(string providerSubscriptionId, CancellationToken ct = default);
}
