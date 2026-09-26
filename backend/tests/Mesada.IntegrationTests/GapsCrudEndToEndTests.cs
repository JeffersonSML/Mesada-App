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
/// Cobre os gaps de CRUD identificados numa auditoria da API: remoção de
/// filho, dados da própria família, convite/resgate de um segundo
/// responsável (Master) e ativar/desativar destinatário de notificação.
/// </summary>
public sealed class GapsCrudEndToEndTests : IClassFixture<MesadaWebApplicationFactory>, IAsyncLifetime
{
    private const string SenhaMasterA = "Teste@123";

    private readonly MesadaWebApplicationFactory _factory;
    private HttpClient _client = default!;

    private Guid _familiaAId;
    private string _emailMasterA = default!;

    public GapsCrudEndToEndTests(MesadaWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var familiaA = new Familia { Id = Guid.NewGuid(), Nome = "[teste-e2e] Família A - Gaps CRUD" };
        admin.Familias.Add(familiaA);

        _emailMasterA = $"master-gaps-{Guid.NewGuid():N}@teste.local";
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
    public async Task RemoverFilho_DeixaDeAparecerNaListagem()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var criarResponse = await _client.PostAsJsonAsync("/api/filhos",
            new CriarFilhoRequest("Filho a Remover", null, CicloPeriodicidade.Mensal, 50m, null));
        var criado = await criarResponse.Content.ReadFromJsonAsync<FilhoDetalheResponse>();

        var removerResponse = await _client.DeleteAsync($"/api/filhos/{criado!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, removerResponse.StatusCode);

        var listaResponse = await _client.GetAsync("/api/filhos");
        var lista = await listaResponse.Content.ReadFromJsonAsync<List<FilhoResponse>>();
        Assert.DoesNotContain(lista!, f => f.Id == criado.Id);
    }

    [Fact]
    public async Task ObterEAtualizarMinhaFamilia_FuncionaDePontaAPonta()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var obterResponse = await _client.GetAsync("/api/familias");
        Assert.Equal(HttpStatusCode.OK, obterResponse.StatusCode);
        var familia = await obterResponse.Content.ReadFromJsonAsync<FamiliaResponse>();
        Assert.Equal(_familiaAId, familia!.Id);

        var atualizarResponse = await _client.PutAsJsonAsync("/api/familias",
            new AtualizarFamiliaRequest("[teste-e2e] Família A Renomeada", CicloPeriodicidade.Quinzenal));
        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);
        var atualizada = await atualizarResponse.Content.ReadFromJsonAsync<FamiliaResponse>();
        Assert.Equal("[teste-e2e] Família A Renomeada", atualizada!.Nome);
        Assert.Equal(CicloPeriodicidade.Quinzenal, atualizada.CicloFechamentoPadrao);
    }

    [Fact]
    public async Task AtualizarMinhaFamilia_ComNomeVazio_Retorna400()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PutAsJsonAsync("/api/familias", new AtualizarFamiliaRequest("", CicloPeriodicidade.Mensal));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConvidarEResgatarMaster_CriaSegundoResponsavelNaoFinanceiro()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var convidarResponse = await _client.PostAsJsonAsync("/api/convites",
            new CriarConviteRequest(PapelConvite.Master, null, "Segundo Responsável"));
        Assert.Equal(HttpStatusCode.Created, convidarResponse.StatusCode);
        var convite = await convidarResponse.Content.ReadFromJsonAsync<ConviteResponse>();

        _client.DefaultRequestHeaders.Authorization = null;
        var emailSegundoMaster = $"segundo-master-{Guid.NewGuid():N}@teste.local";
        var resgateResponse = await _client.PostAsJsonAsync(
            $"/api/auth/convites/{convite!.Codigo}/resgatar-master",
            new ResgatarConviteMasterRequest(emailSegundoMaster, "SenhaForte@123"));
        Assert.Equal(HttpStatusCode.OK, resgateResponse.StatusCode);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var listaResponse = await _client.GetAsync("/api/masters");
        var lista = await listaResponse.Content.ReadFromJsonAsync<List<MasterResponse>>();

        var segundoMaster = Assert.Single(lista!, m => m.Email == emailSegundoMaster);
        Assert.False(segundoMaster.IsFinanceiro);
    }

    [Fact]
    public async Task ResgatarConviteMaster_ComSenhaCurta_Retorna400()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var convidarResponse = await _client.PostAsJsonAsync("/api/convites",
            new CriarConviteRequest(PapelConvite.Master, null, "Responsável Senha Curta"));
        var convite = await convidarResponse.Content.ReadFromJsonAsync<ConviteResponse>();

        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync(
            $"/api/auth/convites/{convite!.Codigo}/resgatar-master",
            new ResgatarConviteMasterRequest($"m-{Guid.NewGuid():N}@teste.local", "123"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Master_NaoConsegueDesativarAPropriaConta_Retorna400()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var listaResponse = await _client.GetAsync("/api/masters");
        var lista = await listaResponse.Content.ReadFromJsonAsync<List<MasterResponse>>();
        var proprioMaster = Assert.Single(lista!, m => m.Email == _emailMasterA);

        var response = await _client.DeleteAsync($"/api/masters/{proprioMaster.Id}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Master_NaoConsegueDesativarUnicoFinanceiroAtivo_MasConsegueDesativarSegundoNaoFinanceiro()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var convidarResponse = await _client.PostAsJsonAsync("/api/convites",
            new CriarConviteRequest(PapelConvite.Master, null, "Segundo a Desativar"));
        var convite = await convidarResponse.Content.ReadFromJsonAsync<ConviteResponse>();

        _client.DefaultRequestHeaders.Authorization = null;
        var email = $"desativar-{Guid.NewGuid():N}@teste.local";
        await _client.PostAsJsonAsync($"/api/auth/convites/{convite!.Codigo}/resgatar-master",
            new ResgatarConviteMasterRequest(email, "SenhaForte@123"));

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var listaResponse = await _client.GetAsync("/api/masters");
        var lista = await listaResponse.Content.ReadFromJsonAsync<List<MasterResponse>>();
        var segundoMaster = Assert.Single(lista!, m => m.Email == email);
        var financeiro = Assert.Single(lista!, m => m.Email == _emailMasterA);

        var desativarFinanceiroResponse = await _client.DeleteAsync($"/api/masters/{financeiro.Id}");
        Assert.Equal(HttpStatusCode.BadRequest, desativarFinanceiroResponse.StatusCode);

        var desativarSegundoResponse = await _client.DeleteAsync($"/api/masters/{segundoMaster.Id}");
        Assert.Equal(HttpStatusCode.NoContent, desativarSegundoResponse.StatusCode);
    }

    [Fact]
    public async Task AtualizarDestinatarioNotificacao_DesativaSemRemover()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var criarResponse = await _client.PostAsJsonAsync("/api/notificacoes/destinatarios",
            new AdicionarDestinatarioNotificacaoRequest(TipoDestinatarioNotificacao.Email, "avo@teste.local"));
        var criado = await criarResponse.Content.ReadFromJsonAsync<DestinatarioNotificacaoResponse>();

        var atualizarResponse = await _client.PutAsJsonAsync($"/api/notificacoes/destinatarios/{criado!.Id}",
            new AtualizarDestinatarioNotificacaoRequest(false));
        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);
        var atualizado = await atualizarResponse.Content.ReadFromJsonAsync<DestinatarioNotificacaoResponse>();
        Assert.False(atualizado!.Ativo);

        var listaResponse = await _client.GetAsync("/api/notificacoes/destinatarios");
        var lista = await listaResponse.Content.ReadFromJsonAsync<List<DestinatarioNotificacaoResponse>>();
        Assert.Contains(lista!, d => d.Id == criado.Id && !d.Ativo);
    }

    private async Task<string> LoginMasterAAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/master/login", new { email = _emailMasterA, senha = SenhaMasterA });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return body!.Token;
    }
}
