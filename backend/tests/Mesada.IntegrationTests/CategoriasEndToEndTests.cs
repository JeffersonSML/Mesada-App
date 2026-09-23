using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mesada.Api.Contracts;
using Mesada.Application.Abstractions;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Mesada.IntegrationTests;

/// <summary>Prova o modelo híbrido de categorias: defaults do sistema visíveis mas não editáveis, customizadas da família com CRUD completo.</summary>
public sealed class CategoriasEndToEndTests : IClassFixture<MesadaWebApplicationFactory>, IAsyncLifetime
{
    private const string SenhaMasterA = "Teste@123";

    private readonly MesadaWebApplicationFactory _factory;
    private HttpClient _client = default!;

    private Guid _familiaAId;
    private string _emailMasterA = default!;

    public CategoriasEndToEndTests(MesadaWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var familiaA = new Familia { Id = Guid.NewGuid(), Nome = "[teste-e2e] Família A - Categorias" };
        admin.Familias.Add(familiaA);

        _emailMasterA = $"master-categorias-{Guid.NewGuid():N}@teste.local";
        admin.UsuariosMaster.Add(new UsuarioMaster
        {
            Id = Guid.NewGuid(),
            FamiliaId = familiaA.Id,
            Nome = "Master A",
            Email = _emailMasterA,
            SenhaHash = hasher.Hash(SenhaMasterA),
            IsFinanceiro = true,
        });

        await admin.SaveChangesAsync();
        _familiaAId = familiaA.Id;
    }

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        await admin.Familias.Where(f => f.Id == _familiaAId).ExecuteDeleteAsync();
    }

    [Fact]
    public async Task Listar_TrazDefaultsDoSistemaESemNenhumaCustomizadaAinda()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/categorias");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var categorias = await response.Content.ReadFromJsonAsync<List<CategoriaResponse>>();

        Assert.NotNull(categorias);
        Assert.NotEmpty(categorias!);
        Assert.All(categorias!, c => Assert.True(c.Sistema));
    }

    [Fact]
    public async Task CriarAtualizarERemover_CategoriaCustomizada_FuncionaDePontaAPonta()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var criarResponse = await _client.PostAsJsonAsync("/api/categorias", new CriarCategoriaRequest("Estudos Extras", null));
        Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);
        var criada = await criarResponse.Content.ReadFromJsonAsync<CategoriaResponse>();
        Assert.False(criada!.Sistema);

        var atualizarResponse = await _client.PutAsJsonAsync($"/api/categorias/{criada.Id}", new AtualizarCategoriaRequest("Estudos Extras Renomeado"));
        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);
        var atualizada = await atualizarResponse.Content.ReadFromJsonAsync<CategoriaResponse>();
        Assert.Equal("Estudos Extras Renomeado", atualizada!.Nome);

        var removerResponse = await _client.DeleteAsync($"/api/categorias/{criada.Id}");
        Assert.Equal(HttpStatusCode.NoContent, removerResponse.StatusCode);

        var listaResponse = await _client.GetAsync("/api/categorias");
        var lista = await listaResponse.Content.ReadFromJsonAsync<List<CategoriaResponse>>();
        Assert.DoesNotContain(lista!, c => c.Id == criada.Id);
    }

    [Fact]
    public async Task Atualizar_CategoriaDoSistema_Retorna400()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var listaResponse = await _client.GetAsync("/api/categorias");
        var lista = await listaResponse.Content.ReadFromJsonAsync<List<CategoriaResponse>>();
        var categoriaSistema = lista!.First(c => c.Sistema);

        var response = await _client.PutAsJsonAsync($"/api/categorias/{categoriaSistema.Id}", new AtualizarCategoriaRequest("Tentando Renomear"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Remover_CategoriaDoSistema_Retorna400()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var listaResponse = await _client.GetAsync("/api/categorias");
        var lista = await listaResponse.Content.ReadFromJsonAsync<List<CategoriaResponse>>();
        var categoriaSistema = lista!.First(c => c.Sistema);

        var response = await _client.DeleteAsync($"/api/categorias/{categoriaSistema.Id}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<string> LoginMasterAAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/master/login", new { email = _emailMasterA, senha = SenhaMasterA });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return body!.Token;
    }
}
