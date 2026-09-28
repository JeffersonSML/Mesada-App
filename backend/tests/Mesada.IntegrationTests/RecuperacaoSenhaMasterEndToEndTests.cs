using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Mesada.Api.Contracts;
using Mesada.Application.Abstractions;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Mesada.IntegrationTests;

/// <summary>
/// Prova de ponta a ponta as três funcionalidades que faltavam para o Master
/// gerenciar a própria senha (lacuna encontrada numa auditoria funcional —
/// só o Administrador tinha isso): "esqueci minha senha" (com envio real de
/// e-mail contra um servidor local que imita o contrato do Resend, mesmo
/// raciocínio de ResendEmailSenderTests), redefinição via token, e troca de
/// senha logado.
/// </summary>
public sealed partial class RecuperacaoSenhaMasterEndToEndTests : IAsyncLifetime
{
    private const string SenhaOriginal = "SenhaOriginal@123";

    private WebApplication _resendFalso = default!;
    private readonly List<string> _corposEmailRecebidos = [];
    private MesadaWebApplicationFactory _factory = default!;
    private HttpClient _client = default!;

    private Guid _familiaId;
    private Guid _masterId;
    private string _emailMaster = default!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        _resendFalso = builder.Build();

        _resendFalso.MapPost("/emails", async context =>
        {
            using var reader = new StreamReader(context.Request.Body);
            _corposEmailRecebidos.Add(await reader.ReadToEndAsync());
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($$"""{"id":"{{Guid.NewGuid()}}"}""");
        });

        await _resendFalso.StartAsync();
        var enderecoResendFalso = _resendFalso.Services
            .GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!
            .Addresses.First();

        _factory = new MesadaWebApplicationFactory(configuracaoExtra: new Dictionary<string, string?>
        {
            ["Email:ResendApiKey"] = "chave-fake-para-teste-local",
            ["Email:ResendApiUrl"] = enderecoResendFalso + "/",
            ["App:UrlBaseWeb"] = "https://teste.mesadaapp.local",
        });
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var familia = new Familia { Id = Guid.NewGuid(), Nome = "[teste-e2e] Família Recuperação Senha" };
        admin.Familias.Add(familia);

        _emailMaster = $"master-recuperacao-{Guid.NewGuid():N}@teste.local";
        var master = new UsuarioMaster
        {
            Id = Guid.NewGuid(),
            FamiliaId = familia.Id,
            Nome = "Master Recuperação",
            Email = _emailMaster,
            SenhaHash = hasher.Hash(SenhaOriginal),
            IsFinanceiro = true,
        };
        admin.UsuariosMaster.Add(master);
        await admin.SaveChangesAsync();

        _familiaId = familia.Id;
        _masterId = master.Id;
    }

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        await admin.Familias.Where(f => f.Id == _familiaId).ExecuteDeleteAsync();

        _factory.Dispose();
        await _resendFalso.StopAsync();
    }

    [Fact]
    public async Task FluxoCompleto_EsqueciSenha_RedefinirSenha_LoginComNovaSenha()
    {
        var esqueciResponse = await _client.PostAsJsonAsync(
            "/api/auth/master/esqueci-senha", new { email = _emailMaster }, MesadaJsonOptions.Default);
        Assert.Equal(HttpStatusCode.NoContent, esqueciResponse.StatusCode);

        Assert.Single(_corposEmailRecebidos);
        var token = ExtrairToken(_corposEmailRecebidos[0]);
        Assert.Contains("https://teste.mesadaapp.local/redefinir-senha?token=", _corposEmailRecebidos[0]);

        const string novaSenha = "SenhaNova@456";
        var redefinirResponse = await _client.PostAsJsonAsync(
            "/api/auth/master/redefinir-senha", new { token, novaSenha }, MesadaJsonOptions.Default);
        Assert.Equal(HttpStatusCode.NoContent, redefinirResponse.StatusCode);

        var loginAntigaResponse = await _client.PostAsJsonAsync(
            "/api/auth/master/login", new { email = _emailMaster, senha = SenhaOriginal }, MesadaJsonOptions.Default);
        Assert.Equal(HttpStatusCode.Unauthorized, loginAntigaResponse.StatusCode);

        var loginNovaResponse = await _client.PostAsJsonAsync(
            "/api/auth/master/login", new { email = _emailMaster, senha = novaSenha }, MesadaJsonOptions.Default);
        Assert.Equal(HttpStatusCode.OK, loginNovaResponse.StatusCode);
    }

    [Fact]
    public async Task EsqueciSenha_ComEmailInexistente_Retorna204SemEnviarEmail()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/master/esqueci-senha", new { email = "ninguem-existe@teste.local" }, MesadaJsonOptions.Default);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(_corposEmailRecebidos);
    }

    [Fact]
    public async Task RedefinirSenha_ComTokenInvalido_Retorna400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/master/redefinir-senha",
            new { token = "token-que-nunca-existiu", novaSenha = "SenhaValida@123" },
            MesadaJsonOptions.Default);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RedefinirSenha_ComSenhaCurta_Retorna400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/master/redefinir-senha",
            new { token = "qualquer-coisa", novaSenha = "curta" },
            MesadaJsonOptions.Default);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RedefinirSenha_UsandoOMesmoTokenDuasVezes_SegundaVezRetorna400()
    {
        await _client.PostAsJsonAsync("/api/auth/master/esqueci-senha", new { email = _emailMaster }, MesadaJsonOptions.Default);
        var token = ExtrairToken(_corposEmailRecebidos[^1]);

        var primeira = await _client.PostAsJsonAsync(
            "/api/auth/master/redefinir-senha", new { token, novaSenha = "PrimeiraTroca@123" }, MesadaJsonOptions.Default);
        Assert.Equal(HttpStatusCode.NoContent, primeira.StatusCode);

        var segunda = await _client.PostAsJsonAsync(
            "/api/auth/master/redefinir-senha", new { token, novaSenha = "SegundaTroca@456" }, MesadaJsonOptions.Default);
        Assert.Equal(HttpStatusCode.BadRequest, segunda.StatusCode);
    }

    [Fact]
    public async Task TrocarSenha_Logado_ComSenhaAtualCorreta_TrocaComSucesso()
    {
        var token = await LoginAsync(SenhaOriginal);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        const string novaSenha = "OutraSenhaNova@789";
        var response = await _client.PostAsJsonAsync(
            "/api/auth/master/trocar-senha",
            new { senhaAtual = SenhaOriginal, novaSenha },
            MesadaJsonOptions.Default);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
        var loginComNova = await _client.PostAsJsonAsync(
            "/api/auth/master/login", new { email = _emailMaster, senha = novaSenha }, MesadaJsonOptions.Default);
        Assert.Equal(HttpStatusCode.OK, loginComNova.StatusCode);
    }

    [Fact]
    public async Task TrocarSenha_ComSenhaAtualErrada_Retorna401()
    {
        var token = await LoginAsync(SenhaOriginal);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/master/trocar-senha",
            new { senhaAtual = "senha-errada", novaSenha = "QualquerSenha@123" },
            MesadaJsonOptions.Default);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TrocarSenha_SemToken_Retorna401()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/master/trocar-senha",
            new { senhaAtual = SenhaOriginal, novaSenha = "QualquerSenha@123" },
            MesadaJsonOptions.Default);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<string> LoginAsync(string senha)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/master/login", new { email = _emailMaster, senha }, MesadaJsonOptions.Default);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>(MesadaJsonOptions.Default);
        return body!.Token;
    }

    private static string ExtrairToken(string corpoEmail)
    {
        var match = RegexToken().Match(corpoEmail);
        Assert.True(match.Success, $"Token não encontrado no corpo do e-mail: {corpoEmail}");
        return match.Groups[1].Value;
    }

    [GeneratedRegex(@"token=([A-Za-z0-9_-]+)")]
    private static partial Regex RegexToken();
}
