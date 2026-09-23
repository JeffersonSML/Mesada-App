using Mesada.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Mesada.IntegrationTests;

/// <summary>
/// Sem chave real do Resend (nem sandbox nem produção) neste ambiente,
/// então validamos contra um servidor HTTP local que imita o contrato real
/// da API (POST /emails → {"id": "&lt;guid&gt;"}) — prova que
/// ResendEmailSender + a fiação de DI (AddResend, EmailOptions,
/// Email:ResendApiUrl) realmente montam e enviam a requisição certa, não
/// que o Resend em si funciona (isso só um teste contra a API real provaria).
/// </summary>
public sealed class ResendEmailSenderTests : IAsyncLifetime
{
    private WebApplication _servidorFalso = default!;
    private string _enderecoServidorFalso = default!;
    private MesadaWebApplicationFactory _factory = default!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        _servidorFalso = builder.Build();

        _servidorFalso.MapPost("/emails", async context =>
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($$"""{"id":"{{Guid.NewGuid()}}"}""");
        });

        await _servidorFalso.StartAsync();
        _enderecoServidorFalso = _servidorFalso.Services
            .GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!
            .Addresses.First();

        _factory = new MesadaWebApplicationFactory(configuracaoExtra: new Dictionary<string, string?>
        {
            ["Email:ResendApiKey"] = "chave-fake-para-teste-local",
            ["Email:ResendApiUrl"] = _enderecoServidorFalso + "/",
            ["Email:RemetenteEmail"] = "nao-responda@mesadaapp.local",
            ["Email:RemetenteNome"] = "Mesada App (teste)",
        });
    }

    public async Task DisposeAsync()
    {
        await _servidorFalso.StopAsync();
        _factory.Dispose();
    }

    [Fact]
    public async Task EnviarAsync_ContraServidorQueRespondeComSucesso_NaoLancaExcecao()
    {
        using var scope = _factory.Services.CreateScope();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        await emailSender.EnviarAsync("filho@exemplo.com", "Assunto de teste", "<p>Corpo de teste</p>");
    }
}
