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
/// Prova, batendo na Api real e no Postgres real, que login → JWT →
/// app.current_familia_id → RLS formam uma cadeia íntegra
/// (docs/adr/0002-schema-e-rls.md): um Master só enxerga os filhos da
/// própria família através do endpoint autenticado, mesmo com outra família
/// e outro filho presentes no mesmo banco.
/// </summary>
public sealed class AutenticacaoETenantEndToEndTests : IClassFixture<MesadaWebApplicationFactory>, IAsyncLifetime
{
    private const string SenhaMasterA = "Teste@123";

    private readonly MesadaWebApplicationFactory _factory;
    private HttpClient _client = default!;

    private Guid _familiaAId;
    private Guid _familiaBId;
    private Guid _filhoAId;
    private Guid _filhoBId;
    private string _emailMasterA = default!;

    public AutenticacaoETenantEndToEndTests(MesadaWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var familiaA = new Familia { Id = Guid.NewGuid(), Nome = "[teste-e2e] Família A" };
        var familiaB = new Familia { Id = Guid.NewGuid(), Nome = "[teste-e2e] Família B" };
        admin.Familias.AddRange(familiaA, familiaB);

        _emailMasterA = $"master-{Guid.NewGuid():N}@teste.local";
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
    public async Task LoginMaster_SoEnxergaOFilhoDaPropriaFamilia()
    {
        var token = await LoginMasterAAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var filhosResponse = await _client.GetAsync("/api/filhos");

        Assert.Equal(HttpStatusCode.OK, filhosResponse.StatusCode);
        var filhos = await filhosResponse.Content.ReadFromJsonAsync<List<FilhoResponse>>();

        Assert.NotNull(filhos);
        Assert.Contains(filhos!, f => f.Id == _filhoAId);
        Assert.DoesNotContain(filhos!, f => f.Id == _filhoBId);
    }

    [Fact]
    public async Task LoginMaster_ComSenhaErrada_Retorna401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/master/login", new { email = _emailMasterA, senha = "senha-errada" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LoginMaster_ComEmailInexistente_Retorna401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/master/login", new { email = "ninguem@teste.local", senha = "qualquer" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Filhos_SemToken_Retorna401()
    {
        var response = await _client.GetAsync("/api/filhos");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<string> LoginMasterAAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/master/login", new { email = _emailMasterA, senha = SenhaMasterA });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return body!.Token;
    }
}
