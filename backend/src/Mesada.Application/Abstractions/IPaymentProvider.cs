namespace Mesada.Application.Abstractions;

/// <summary>
/// Abstrai o gateway de pagamento (Asaas ou Stripe — docs/especificacao.md
/// #planos-e-assinatura). Implementações concretas ficam em
/// Mesada.Infrastructure e são plugadas na Etapa 6; nenhum código de
/// Application ou Api deve referenciar um SDK de gateway diretamente.
/// </summary>
public interface IPaymentProvider
{
    Task<string> CriarClienteAsync(Guid familiaId, string nome, string email, CancellationToken ct = default);

    Task<string> CriarAssinaturaAsync(string providerCustomerId, string planoCodigo, CancellationToken ct = default);

    Task CancelarAssinaturaAsync(string providerSubscriptionId, CancellationToken ct = default);
}
