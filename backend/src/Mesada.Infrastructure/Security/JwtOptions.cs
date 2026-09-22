namespace Mesada.Infrastructure.Security;

/// <summary>
/// Lido de configuração (appsettings / variáveis de ambiente), nunca
/// hardcoded. Em produção, Chave deve vir de um cofre de segredos, nunca de
/// appsettings.json versionado.
/// </summary>
public sealed class JwtOptions
{
    public const string SecaoConfiguracao = "Jwt";

    public string Chave { get; set; } = default!;
    public string Emissor { get; set; } = "mesada-app";
    public string Audiencia { get; set; } = "mesada-app-clients";
    public int ExpiracaoMinutosMaster { get; set; } = 60;
    public int ExpiracaoMinutosComum { get; set; } = 60 * 24 * 30; // 30 dias — o filho fica logado no dispositivo vinculado
    public int ExpiracaoMinutosAdministrador { get; set; } = 30;
}
