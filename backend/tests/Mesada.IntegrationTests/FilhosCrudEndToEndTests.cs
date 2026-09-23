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

/// <summary>Prova o CRUD de filhos (UsuarioComum) de ponta a ponta, incluindo validação e isolamento por RLS.</summary>
public sealed class FilhosCrudEndToEndTests : IClassFixture<MesadaWebApplicationFactory>, IAsyncLifetime
{
    private const string SenhaMasterA = "Teste@123";

    private readonly MesadaWebApplicationFactory _factory;
    private HttpClient _client = default!;

    private Guid _familiaAId;
    private Guid _familiaBId;
    private Guid _filhoBId;
    private string _emailMasterA = default!;

    public FilhosCrudEndToEndTests(MesadaWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var familiaA = new Familia { Id = Guid.NewGuid(), Nome = "[teste-e2e] Família A - Filhos CRUD" };
        var familiaB = new Familia { Id = Guid.NewGuid(), Nome = "[teste-e2e] Família B - Filhos CRUD" };
        admin.Familias.AddRange(familiaA, familiaB);

        _emailMasterA = $"master-filhos-{Guid.NewGuid():N}@teste.local";
        admin.UsuariosMaster.Add(new UsuarioMaster
        {
            Id = Guid.NewGuid(),
            FamiliaId = familiaA.Id,
            Nome = "Master A",
            Email = _emailMasterA,
            SenhaHash = hasher.Hash(SenhaMasterA),
            IsFinanceiro = true,
        });

        var filhoB = new UsuarioComum { Id = Guid.NewGuid(), FamiliaId = familiaB.Id, Nome = "[teste-e2e] Filho B" };
        admin.UsuariosComuns.Add(filhoB);

        await admin.SaveChangesAsync();

        _familiaAId = familiaA.Id;
        _familiaBId = familiaB.Id;
        _filhoBId = filhoB.Id;
    }

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        await admin.Familias.Where(f => f.Id == _familiaAId || f.Id == _familiaBId).ExecuteDeleteAsync();
    }

    [Fact]
    public async Task CriarEAtualizar_PersisteMesadaBaseEValorPonto()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var criarResponse = await _client.PostAsJsonAsync("/api/filhos",
            new CriarFilhoRequest("Filho A", "Fofo", CicloPeriodicidade.Mensal, 100m, 0.5m));
        Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);
        var criado = await criarResponse.Content.ReadFromJsonAsync<FilhoDetalheResponse>();
        Assert.Equal(100m, criado!.MesadaBase);
        Assert.Equal(0.5m, criado.ValorPonto);

        var atualizarResponse = await _client.PutAsJsonAsync($"/api/filhos/{criado.Id}",
            new AtualizarFilhoRequest("Filho A Renomeado", "Fofinho", CicloPeriodicidade.Quinzenal, 150m, null));
        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);
        var atualizado = await atualizarResponse.Content.ReadFromJsonAsync<FilhoDetalheResponse>();
        Assert.Equal("Filho A Renomeado", atualizado!.Nome);
        Assert.Equal(150m, atualizado.MesadaBase);
        Assert.Null(atualizado.ValorPonto);
        Assert.Equal(CicloPeriodicidade.Quinzenal, atualizado.CicloFechamento);
    }

    [Fact]
    public async Task Criar_ComMesadaBaseNegativa_Retorna400()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/filhos",
            new CriarFilhoRequest("Filho Invalido", null, CicloPeriodicidade.Mensal, -10m, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Atualizar_DeFilhoDeOutraFamilia_Retorna404()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PutAsJsonAsync($"/api/filhos/{_filhoBId}",
            new AtualizarFilhoRequest("Tentativa", null, CicloPeriodicidade.Mensal, 50m, null));

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
