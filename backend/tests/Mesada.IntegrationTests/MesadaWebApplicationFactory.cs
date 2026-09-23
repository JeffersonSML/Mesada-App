using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Mesada.IntegrationTests;

/// <summary>
/// Sobe a Api real (Program.cs) contra o PostgreSQL local já provisionado
/// por infra/db/scripts/provisionar.sh — sem mocks de banco: o objetivo
/// desta suíte é provar que autenticação + tenant context + RLS funcionam
/// juntos de ponta a ponta, exatamente como em produção.
/// </summary>
public sealed class MesadaWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly IReadOnlyDictionary<string, string?> _configuracaoExtra;

    /// <summary>Construtor exigido pelo xUnit para uso como IClassFixture — não aceita parâmetro, senão a ativação automática do fixture falha (unresolved constructor arguments).</summary>
    public MesadaWebApplicationFactory() : this(new Dictionary<string, string?>())
    {
    }

    /// <summary>
    /// Uso direto (fora de IClassFixture), quando o teste precisa customizar
    /// configuração — ver ResendEmailSenderTests. `internal`, não `public`:
    /// xUnit exige que um IClassFixture tenha exatamente um construtor
    /// público, então o segundo construtor fica só visível dentro deste
    /// assembly de teste.
    /// </summary>
    internal MesadaWebApplicationFactory(IReadOnlyDictionary<string, string?> configuracaoExtra)
    {
        _configuracaoExtra = configuracaoExtra;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var appConnection = Environment.GetEnvironmentVariable("MESADA_TEST_APP_CONNECTION")
                ?? "Host=127.0.0.1;Port=5432;Database=mesada;Username=mesada_app;Password=mesada_app_dev_pw;Pooling=false";
            var adminConnection = Environment.GetEnvironmentVariable("MESADA_TEST_ADMIN_CONNECTION")
                ?? "Host=127.0.0.1;Port=5432;Database=mesada;Username=mesada_admin;Password=mesada_admin_dev_pw";

            var configuracaoBase = new Dictionary<string, string?>
            {
                ["ConnectionStrings:MesadaApp"] = appConnection,
                ["ConnectionStrings:MesadaAdmin"] = adminConnection,
                // Chave só para este processo de teste — nunca reutilizar em produção.
                ["Jwt:Chave"] = "chave-de-integracao-somente-para-teste-local-nao-usar-em-producao-32bytes+",
            };

            foreach (var (chave, valor) in _configuracaoExtra)
                configuracaoBase[chave] = valor;

            config.AddInMemoryCollection(configuracaoBase);
        });
    }
}
