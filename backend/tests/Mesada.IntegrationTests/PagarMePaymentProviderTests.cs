using Mesada.Infrastructure.Payments;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PagarMe;
using Xunit;

namespace Mesada.IntegrationTests;

/// <summary>
/// Sem uma chave de sandbox do Pagar.me neste ambiente (ver
/// docs/adr/0006-*.md), validamos PagarMePaymentProvider contra um
/// servidor HTTP local que imita o formato real de resposta da API v5
/// (GetCustomerResponse com Id) — prova que o adapter monta a requisição e
/// interpreta BaseResponse/IsSuccessfully corretamente, não que a API real
/// do Pagar.me se comporta assim (só uma chamada real provaria isso).
/// </summary>
public sealed class PagarMePaymentProviderTests : IAsyncLifetime
{
    private WebApplication _servidorFalso = default!;
    private PagarMePaymentProvider _provider = default!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        _servidorFalso = builder.Build();

        _servidorFalso.MapPost("/customers", async (HttpContext context) =>
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($$"""{"id":"cus_fake_123","name":"Família Teste","email":"teste@exemplo.com"}""");
        });

        await _servidorFalso.StartAsync();
        var endereco = _servidorFalso.Services
            .GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!
            .Addresses.First();

        var configuration = new Configuration(
            secretKey: "sk_test_fake",
            requestKey: null,
            apiUrl: endereco,
            timeout: null,
            mpToken: null,
            accountManagementKey: null,
            enableLog: false);
        IPagarMeApiClient client = new PagarMeApiClient(configuration);
        _provider = new PagarMePaymentProvider(client);
    }

    public async Task DisposeAsync() => await _servidorFalso.StopAsync();

    [Fact]
    public async Task CriarClienteAsync_ContraServidorQueRespondeComSucesso_RetornaId()
    {
        var id = await _provider.CriarClienteAsync(Guid.NewGuid(), "Família Teste", "teste@exemplo.com");

        Assert.Equal("cus_fake_123", id);
    }
}
