using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mesada.Api.Contracts;
using Mesada.Application.Abstractions;
using Mesada.Domain.Entities;
using Mesada.Domain.Enums;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Mesada.IntegrationTests;

/// <summary>
/// Prova, batendo na Api real e no Postgres real, que a lista de
/// destinatários extras de notificação (docs/especificacao.md
/// #notificações) respeita RLS e as regras de validação de formato — a
/// funcionalidade nasceu de uma pergunta direta do usuário sobre se
/// haveria interface para add/del de e-mail e telefone.
/// </summary>
public sealed class DestinatariosNotificacaoEndToEndTests : IClassFixture<MesadaWebApplicationFactory>, IAsyncLifetime
{
    private const string SenhaMasterA = "Teste@123";

    private readonly MesadaWebApplicationFactory _factory;
    private HttpClient _client = default!;

    private Guid _familiaAId;
    private Guid _familiaBId;
    private string _emailMasterA = default!;

    public DestinatariosNotificacaoEndToEndTests(MesadaWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var familiaA = new Familia { Id = Guid.NewGuid(), Nome = "[teste-e2e] Família A - Notificacoes" };
        var familiaB = new Familia { Id = Guid.NewGuid(), Nome = "[teste-e2e] Família B - Notificacoes" };
        admin.Familias.AddRange(familiaA, familiaB);

        _emailMasterA = $"master-notif-{Guid.NewGuid():N}@teste.local";
        var masterA = new UsuarioMaster
        {
            Id = Guid.NewGuid(),
            FamiliaId = familiaA.Id,
            Nome = "Master A",
            Email = _emailMasterA,
            SenhaHash = hasher.Hash(SenhaMasterA),
            IsFinanceiro = true,
        };
        admin.UsuariosMaster.Add(masterA);

        // Destinatário pré-existente na Família B — prova de isolamento por RLS.
        admin.NotificacaoDestinatarios.Add(new NotificacaoDestinatario
        {
            Id = Guid.NewGuid(),
            FamiliaId = familiaB.Id,
            Tipo = TipoDestinatarioNotificacao.Email,
            Valor = "outrafamilia@teste.local",
        });

        await admin.SaveChangesAsync();

        _familiaAId = familiaA.Id;
        _familiaBId = familiaB.Id;
    }

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        await admin.Familias.Where(f => f.Id == _familiaAId || f.Id == _familiaBId).ExecuteDeleteAsync();
    }

    [Fact]
    public async Task AdicionarEListar_SoEnxergaDestinatariosDaPropriaFamilia()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var criarResponse = await _client.PostAsJsonAsync("/api/notificacoes/destinatarios",
            new AdicionarDestinatarioNotificacaoRequest(TipoDestinatarioNotificacao.Email, "avo@teste.local"));
        Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);

        var listaResponse = await _client.GetAsync("/api/notificacoes/destinatarios");
        Assert.Equal(HttpStatusCode.OK, listaResponse.StatusCode);
        var lista = await listaResponse.Content.ReadFromJsonAsync<List<DestinatarioNotificacaoResponse>>();

        Assert.NotNull(lista);
        Assert.Single(lista!);
        Assert.Equal("avo@teste.local", lista![0].Valor);
    }

    [Fact]
    public async Task Adicionar_ComEmailInvalido_Retorna400()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/notificacoes/destinatarios",
            new AdicionarDestinatarioNotificacaoRequest(TipoDestinatarioNotificacao.Email, "nao-e-um-email"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Adicionar_ComTelefoneForaDoFormatoE164_Retorna400()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/notificacoes/destinatarios",
            new AdicionarDestinatarioNotificacaoRequest(TipoDestinatarioNotificacao.Telefone, "11999999999"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AdicionarERemover_RemoveComSucesso()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var criarResponse = await _client.PostAsJsonAsync("/api/notificacoes/destinatarios",
            new AdicionarDestinatarioNotificacaoRequest(TipoDestinatarioNotificacao.Telefone, "+5511999999999"));
        var criado = await criarResponse.Content.ReadFromJsonAsync<DestinatarioNotificacaoResponse>();

        var removerResponse = await _client.DeleteAsync($"/api/notificacoes/destinatarios/{criado!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, removerResponse.StatusCode);

        var listaResponse = await _client.GetAsync("/api/notificacoes/destinatarios");
        var lista = await listaResponse.Content.ReadFromJsonAsync<List<DestinatarioNotificacaoResponse>>();
        Assert.Empty(lista!);
    }

    [Fact]
    public async Task Remover_DeIdInexistente_Retorna404()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.DeleteAsync($"/api/notificacoes/destinatarios/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<string> LoginMasterAAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/master/login", new { email = _emailMasterA, senha = SenhaMasterA });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return body!.Token;
    }
}
