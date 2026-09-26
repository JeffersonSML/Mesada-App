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
/// Prova de ponta a ponta que um convite criado pelo Master (POST
/// /api/convites) é, de fato, resgatável pelo endpoint público existente
/// (POST /api/auth/convites/{codigo}/resgatar), fechando o ciclo completo
/// do fluxo de convite (docs/especificacao.md#fluxo-de-convite).
/// </summary>
public sealed class ConvitesEndToEndTests : IClassFixture<MesadaWebApplicationFactory>, IAsyncLifetime
{
    private const string SenhaMasterA = "Teste@123";

    private readonly MesadaWebApplicationFactory _factory;
    private HttpClient _client = default!;

    private Guid _familiaAId;
    private Guid _familiaBId;
    private Guid _filhoAId;
    private Guid _filhoBId;
    private string _emailMasterA = default!;

    public ConvitesEndToEndTests(MesadaWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var familiaA = new Familia { Id = Guid.NewGuid(), Nome = "[teste-e2e] Família A - Convites" };
        var familiaB = new Familia { Id = Guid.NewGuid(), Nome = "[teste-e2e] Família B - Convites" };
        admin.Familias.AddRange(familiaA, familiaB);

        _emailMasterA = $"master-convites-{Guid.NewGuid():N}@teste.local";
        admin.UsuariosMaster.Add(new UsuarioMaster
        {
            Id = Guid.NewGuid(),
            FamiliaId = familiaA.Id,
            Nome = "Master A",
            Email = _emailMasterA,
            SenhaHash = hasher.Hash(SenhaMasterA),
            IsFinanceiro = true,
        });

        var filhoA = new UsuarioComum { Id = Guid.NewGuid(), FamiliaId = familiaA.Id, Nome = "[teste-e2e] Filho A" };
        var filhoB = new UsuarioComum { Id = Guid.NewGuid(), FamiliaId = familiaB.Id, Nome = "[teste-e2e] Filho B" };
        admin.UsuariosComuns.AddRange(filhoA, filhoB);

        await admin.SaveChangesAsync();

        _familiaAId = familiaA.Id;
        _familiaBId = familiaB.Id;
        _filhoAId = filhoA.Id;
        _filhoBId = filhoB.Id;
    }

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        await admin.Familias.Where(f => f.Id == _familiaAId || f.Id == _familiaBId).ExecuteDeleteAsync();
    }

    [Fact]
    public async Task CriarConvite_EResgatarPeloAppMobile_VinculaODispositivo()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var criarResponse = await _client.PostAsJsonAsync("/api/convites", new CriarConviteRequest(PapelConvite.Comum, _filhoAId, null));
        Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);
        var convite = await criarResponse.Content.ReadFromJsonAsync<ConviteResponse>();
        Assert.Equal(StatusConvite.Pendente, convite!.Status);

        _client.DefaultRequestHeaders.Authorization = null;
        var resgateResponse = await _client.PostAsJsonAsync(
            $"/api/auth/convites/{convite.Codigo}/resgatar", new ResgatarConviteRequest("dispositivo-teste-123"));
        Assert.Equal(HttpStatusCode.OK, resgateResponse.StatusCode);
        var tokenFilho = await resgateResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.False(string.IsNullOrWhiteSpace(tokenFilho!.Token));
    }

    [Fact]
    public async Task CriarConvite_ParaFilhoDeOutraFamilia_Retorna404()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/convites", new CriarConviteRequest(PapelConvite.Comum, _filhoBId, null));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Listar_SoEnxergaConvitesDaPropriaFamilia()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/convites", new CriarConviteRequest(PapelConvite.Comum, _filhoAId, null));

        var listaResponse = await _client.GetAsync("/api/convites");
        var lista = await listaResponse.Content.ReadFromJsonAsync<List<ConviteResponse>>();

        Assert.NotNull(lista);
        Assert.Single(lista!);
        Assert.Equal(_filhoAId, lista![0].UsuarioComumId);
    }

    [Fact]
    public async Task Revogar_ImpedeResgateSubsequente()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var criarResponse = await _client.PostAsJsonAsync("/api/convites", new CriarConviteRequest(PapelConvite.Comum, _filhoAId, null));
        var convite = await criarResponse.Content.ReadFromJsonAsync<ConviteResponse>();

        var revogarResponse = await _client.DeleteAsync($"/api/convites/{convite!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, revogarResponse.StatusCode);

        var revogarNovamenteResponse = await _client.DeleteAsync($"/api/convites/{convite.Id}");
        Assert.Equal(HttpStatusCode.BadRequest, revogarNovamenteResponse.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
        var resgateResponse = await _client.PostAsJsonAsync(
            $"/api/auth/convites/{convite.Codigo}/resgatar", new ResgatarConviteRequest("dispositivo-teste-456"));
        Assert.Equal(HttpStatusCode.BadRequest, resgateResponse.StatusCode);
    }

    private async Task<string> LoginMasterAAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/master/login", new { email = _emailMasterA, senha = SenhaMasterA });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return body!.Token;
    }
}
