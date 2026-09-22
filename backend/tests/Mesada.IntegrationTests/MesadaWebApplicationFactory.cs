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
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var appConnection = Environment.GetEnvironmentVariable("MESADA_TEST_APP_CONNECTION")
                ?? "Host=127.0.0.1;Port=5432;Database=mesada;Username=mesada_app;Password=mesada_app_dev_pw;Pooling=false";
            var adminConnection = Environment.GetEnvironmentVariable("MESADA_TEST_ADMIN_CONNECTION")
                ?? "Host=127.0.0.1;Port=5432;Database=mesada;Username=mesada_admin;Password=mesada_admin_dev_pw";

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MesadaApp"] = appConnection,
                ["ConnectionStrings:MesadaAdmin"] = adminConnection,
                // Chave só para este processo de teste — nunca reutilizar em produção.
                ["Jwt:Chave"] = "chave-de-integracao-somente-para-teste-local-nao-usar-em-producao-32bytes+",
            });
        });
    }
}
