using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mesada.Api.Contracts;
using Mesada.Application.Abstractions;
using Mesada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mesada.Infrastructure.Persistence;
using Xunit;

namespace Mesada.IntegrationTests;

/// <summary>
/// Prova o painel administrativo interno de ponta a ponta (pedido do dono
/// do sistema: acesso total para ele — grupo Owner — e acesso limitado para
/// outras pessoas da equipe em grupos específicos, ex.: "Tecnologia").
///
/// Não testa aqui a regra "não pode desativar o último Owner ativo": o
/// Postgres usado pelos testes é o mesmo banco de desenvolvimento com o
/// Administrador Owner real (criado por
/// infra/db/scripts/criar_administrador_owner.sh) — desativar todos os
/// Owners para forçar esse caminho destruiria o acesso real do usuário.
/// A regra é simples o suficiente para ficar coberta por revisão de código.
/// </summary>
public sealed class AdministracaoEndToEndTests : IClassFixture<MesadaWebApplicationFactory>, IAsyncLifetime
{
    private const string SenhaOwner = "Teste@1234";

    private readonly MesadaWebApplicationFactory _factory;
    private HttpClient _client = default!;

    private Guid _grupoTecnologiaId;
    private Guid _ownerTesteId;
    private string _emailOwnerTeste = default!;
    private string _tokenOwnerTeste = default!;

    public AdministracaoEndToEndTests(MesadaWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var tokens = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        var grupoOwner = await admin.GruposAdministrador.SingleAsync(g => g.Nome == "Owner");

        var grupoTecnologia = new GrupoAdministrador
        {
            Id = Guid.NewGuid(),
            Nome = $"[teste-e2e] Tecnologia {Guid.NewGuid():N}",
            Descricao = "Grupo de teste",
            Permissoes = "{}",
            Sistema = false,
        };
        admin.GruposAdministrador.Add(grupoTecnologia);

        _emailOwnerTeste = $"owner-teste-{Guid.NewGuid():N}@teste.local";
        var ownerTeste = new Administrador
        {
            Id = Guid.NewGuid(),
            Nome = "[teste-e2e] Owner de Teste",
            Email = _emailOwnerTeste,
            SenhaHash = hasher.Hash(SenhaOwner),
            GrupoId = grupoOwner.Id,
            DeveTrocarSenha = false,
            Status = "ativo",
        };
        admin.Administradores.Add(ownerTeste);

        await admin.SaveChangesAsync();

        _grupoTecnologiaId = grupoTecnologia.Id;
        _ownerTesteId = ownerTeste.Id;

        _tokenOwnerTeste = tokens.GerarTokenAdministrador(new Administrador
        {
            Id = ownerTeste.Id,
            GrupoId = grupoOwner.Id,
            Grupo = grupoOwner,
        });
    }

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        await admin.Administradores.Where(a => a.Id == _ownerTesteId || a.GrupoId == _grupoTecnologiaId).ExecuteDeleteAsync();
        await admin.GruposAdministrador.Where(g => g.Id == _grupoTecnologiaId).ExecuteDeleteAsync();
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_Retorna200()
    {
        var response = await _client.PostAsJsonAsync("/api/admin/auth/login", new LoginAdministradorRequest(_emailOwnerTeste, SenhaOwner));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TokenAdministradorResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.False(body.DeveTrocarSenha);
    }

    [Fact]
    public async Task Login_ComSenhaErrada_Retorna401()
    {
        var response = await _client.PostAsJsonAsync("/api/admin/auth/login", new LoginAdministradorRequest(_emailOwnerTeste, "senha-errada"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Owner_ConvidaAdministrador_ESenhaTemporariaFuncionaComExigenciaDeTroca()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenOwnerTeste);

        var email = $"convidado-{Guid.NewGuid():N}@teste.local";
        var convidarResponse = await _client.PostAsJsonAsync("/api/admin/administradores",
            new ConvidarAdministradorRequest("Pessoa de Tecnologia", email, _grupoTecnologiaId));
        Assert.Equal(HttpStatusCode.Created, convidarResponse.StatusCode);
        var convite = await convidarResponse.Content.ReadFromJsonAsync<ConviteAdministradorResponse>();
        Assert.True(convite!.Administrador.DeveTrocarSenha);

        _client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await _client.PostAsJsonAsync("/api/admin/auth/login", new LoginAdministradorRequest(email, convite.SenhaTemporaria));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<TokenAdministradorResponse>();
        Assert.True(loginBody!.DeveTrocarSenha);
    }

    [Fact]
    public async Task NaoOwner_NaoConsegueConvidarAdministrador_Retorna403()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenOwnerTeste);
        var email = $"tecnico-{Guid.NewGuid():N}@teste.local";
        var convidarResponse = await _client.PostAsJsonAsync("/api/admin/administradores",
            new ConvidarAdministradorRequest("Técnico", email, _grupoTecnologiaId));
        var convite = await convidarResponse.Content.ReadFromJsonAsync<ConviteAdministradorResponse>();

        var loginResponse = await _client.PostAsJsonAsync("/api/admin/auth/login", new LoginAdministradorRequest(email, convite!.SenhaTemporaria));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<TokenAdministradorResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody!.Token);
        var response = await _client.PostAsJsonAsync("/api/admin/administradores",
            new ConvidarAdministradorRequest("Outro", $"outro-{Guid.NewGuid():N}@teste.local", _grupoTecnologiaId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Owner_NaoConsegueDesativarAPropriaConta_Retorna400()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenOwnerTeste);
        var response = await _client.DeleteAsync($"/api/admin/administradores/{_ownerTesteId}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GrupoSistema_NaoPodeSerEditadoNemRemovido()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenOwnerTeste);

        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        var grupoOwnerId = await admin.GruposAdministrador.Where(g => g.Nome == "Owner").Select(g => g.Id).SingleAsync();

        var atualizarResponse = await _client.PutAsJsonAsync($"/api/admin/grupos/{grupoOwnerId}",
            new SalvarGrupoAdministradorRequest("Owner Renomeado", null, null));
        Assert.Equal(HttpStatusCode.BadRequest, atualizarResponse.StatusCode);

        var removerResponse = await _client.DeleteAsync($"/api/admin/grupos/{grupoOwnerId}");
        Assert.Equal(HttpStatusCode.BadRequest, removerResponse.StatusCode);
    }

    [Fact]
    public async Task RemoverGrupo_ComAdministradoresVinculados_Retorna400()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenOwnerTeste);

        await _client.PostAsJsonAsync("/api/admin/administradores",
            new ConvidarAdministradorRequest("Vinculado", $"vinculado-{Guid.NewGuid():N}@teste.local", _grupoTecnologiaId));

        var response = await _client.DeleteAsync($"/api/admin/grupos/{_grupoTecnologiaId}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TrocarSenha_ComSenhaAtualErrada_Retorna401()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenOwnerTeste);
        var response = await _client.PostAsJsonAsync("/api/admin/auth/trocar-senha",
            new TrocarSenhaAdministradorRequest("senha-errada", "NovaSenhaForte123"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AtualizarEmail_ParaOMesmoEmailAtual_NaoConflitaConsigoMesmo()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenOwnerTeste);
        var response = await _client.PutAsJsonAsync("/api/admin/auth/email", new AtualizarEmailAdministradorRequest(_emailOwnerTeste));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task AtualizarEmail_ParaEmailDeOutroAdministrador_Retorna409()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenOwnerTeste);
        var emailConvidado = $"convidado-email-{Guid.NewGuid():N}@teste.local";
        var convidarResponse = await _client.PostAsJsonAsync("/api/admin/administradores",
            new ConvidarAdministradorRequest("Convidado", emailConvidado, _grupoTecnologiaId));
        var convite = await convidarResponse.Content.ReadFromJsonAsync<ConviteAdministradorResponse>();

        var loginResponse = await _client.PostAsJsonAsync("/api/admin/auth/login", new LoginAdministradorRequest(emailConvidado, convite!.SenhaTemporaria));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<TokenAdministradorResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody!.Token);
        var response = await _client.PutAsJsonAsync("/api/admin/auth/email", new AtualizarEmailAdministradorRequest(_emailOwnerTeste));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
