namespace Mesada.Infrastructure.Payments;

public sealed class PagarMeOptions
{
    public const string SecaoConfiguracao = "PagarMe";

    /// <summary>Chave secreta (sk_test_... em sandbox, sk_... em produção) — nunca versionada, só via variável de ambiente/cofre.</summary>
    public string SecretKey { get; set; } = default!;
}
