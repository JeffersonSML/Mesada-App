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

/// <summary>Prova o fluxo de conclusão e aprovação de tarefas de ponta a ponta (docs/especificacao.md#fluxo-de-aprovação-de-tarefas).</summary>
public sealed class ExecucoesEndToEndTests : IClassFixture<MesadaWebApplicationFactory>, IAsyncLifetime
{
    private const string SenhaMasterA = "Teste@123";

    private readonly MesadaWebApplicationFactory _factory;
    private HttpClient _client = default!;

    private Guid _familiaAId;
    private Guid _filhoId;
    private Guid _outroFilhoId;
    private Guid _aderenciaComAprovacaoId;
    private Guid _aderenciaSemAprovacaoId;
    private Guid _aderenciaSemParcialId;
    private string _emailMasterA = default!;
    private string _tokenComum = default!;
    private string _tokenOutroComum = default!;

    public ExecucoesEndToEndTests(MesadaWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var tokens = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        var familiaA = new Familia { Id = Guid.NewGuid(), Nome = "[teste-e2e] Família A - Execuções" };
        admin.Familias.Add(familiaA);
        await admin.SaveChangesAsync();

        _emailMasterA = $"master-execucoes-{Guid.NewGuid():N}@teste.local";
        admin.UsuariosMaster.Add(new UsuarioMaster
        {
            Id = Guid.NewGuid(),
            FamiliaId = familiaA.Id,
            Nome = "Master A",
            Email = _emailMasterA,
            SenhaHash = hasher.Hash(SenhaMasterA),
            IsFinanceiro = true,
        });

        var filho = new UsuarioComum { Id = Guid.NewGuid(), FamiliaId = familiaA.Id, Nome = "[teste-e2e] Filho", MesadaBase = 100m };
        var outroFilho = new UsuarioComum { Id = Guid.NewGuid(), FamiliaId = familiaA.Id, Nome = "[teste-e2e] Outro Filho", MesadaBase = 100m };
        admin.UsuariosComuns.AddRange(filho, outroFilho);

        var categoria = new Categoria { Id = Guid.NewGuid(), FamiliaId = familiaA.Id, Nome = "[teste-e2e] Categoria Execuções" };
        admin.Categorias.Add(categoria);

        var tarefaComAprovacao = new Tarefa
        {
            Id = Guid.NewGuid(), FamiliaId = familiaA.Id, CategoriaId = categoria.Id, Nome = "Tarefa com aprovação",
            ModoCalculo = ModoCalculo.ValorDireto, Valor = 20m, Natureza = NaturezaTarefa.Bonus, RequerAprovacao = true,
        };
        var tarefaSemAprovacao = new Tarefa
        {
            Id = Guid.NewGuid(), FamiliaId = familiaA.Id, CategoriaId = categoria.Id, Nome = "Tarefa sem aprovação",
            ModoCalculo = ModoCalculo.ValorDireto, Valor = 5m, Natureza = NaturezaTarefa.Bonus, RequerAprovacao = false,
        };
        var tarefaSemParcial = new Tarefa
        {
            Id = Guid.NewGuid(), FamiliaId = familiaA.Id, CategoriaId = categoria.Id, Nome = "Tarefa sem parcial",
            ModoCalculo = ModoCalculo.ValorDireto, Valor = 8m, Natureza = NaturezaTarefa.Bonus, RequerAprovacao = false, PermiteParcial = false,
        };
        admin.Tarefas.AddRange(tarefaComAprovacao, tarefaSemAprovacao, tarefaSemParcial);
        await admin.SaveChangesAsync();

        var aderenciaComAprovacao = new TarefaUsuario { Id = Guid.NewGuid(), FamiliaId = familiaA.Id, TarefaId = tarefaComAprovacao.Id, UsuarioComumId = filho.Id };
        var aderenciaSemAprovacao = new TarefaUsuario { Id = Guid.NewGuid(), FamiliaId = familiaA.Id, TarefaId = tarefaSemAprovacao.Id, UsuarioComumId = filho.Id };
        var aderenciaSemParcial = new TarefaUsuario { Id = Guid.NewGuid(), FamiliaId = familiaA.Id, TarefaId = tarefaSemParcial.Id, UsuarioComumId = filho.Id };
        admin.TarefasUsuarios.AddRange(aderenciaComAprovacao, aderenciaSemAprovacao, aderenciaSemParcial);
        await admin.SaveChangesAsync();

        _familiaAId = familiaA.Id;
        _filhoId = filho.Id;
        _outroFilhoId = outroFilho.Id;
        _aderenciaComAprovacaoId = aderenciaComAprovacao.Id;
        _aderenciaSemAprovacaoId = aderenciaSemAprovacao.Id;
        _aderenciaSemParcialId = aderenciaSemParcial.Id;
        _tokenComum = tokens.GerarTokenComum(filho, "dispositivo-teste-filho");
        _tokenOutroComum = tokens.GerarTokenComum(outroFilho, "dispositivo-teste-outro-filho");
    }

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        await admin.Familias.Where(f => f.Id == _familiaAId).ExecuteDeleteAsync();
    }

    [Fact]
    public async Task Marcar_TarefaComAprovacao_FicaPendenteAteMasterAprovar()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenComum);

        var marcarResponse = await _client.PostAsJsonAsync("/api/execucoes",
            new MarcarExecucaoRequest(_aderenciaComAprovacaoId, StatusExecucao.Feito, 100m));
        Assert.Equal(HttpStatusCode.Created, marcarResponse.StatusCode);
        var execucao = await marcarResponse.Content.ReadFromJsonAsync<ExecucaoResponse>();
        Assert.Equal(StatusAprovacao.Pendente, execucao!.StatusAprovacao);
        Assert.Equal(20m, execucao.ValorCalculado);

        var tokenMaster = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenMaster);

        var pendentesResponse = await _client.GetAsync("/api/execucoes/pendentes");
        var pendentes = await pendentesResponse.Content.ReadFromJsonAsync<List<ExecucaoResponse>>();
        Assert.Contains(pendentes!, e => e.Id == execucao.Id);

        var aprovarResponse = await _client.PostAsync($"/api/execucoes/{execucao.Id}/aprovar", null);
        Assert.Equal(HttpStatusCode.OK, aprovarResponse.StatusCode);
        var aprovada = await aprovarResponse.Content.ReadFromJsonAsync<ExecucaoResponse>();
        Assert.Equal(StatusAprovacao.Aprovado, aprovada!.StatusAprovacao);

        var aprovarNovamenteResponse = await _client.PostAsync($"/api/execucoes/{execucao.Id}/aprovar", null);
        Assert.Equal(HttpStatusCode.BadRequest, aprovarNovamenteResponse.StatusCode);
    }

    [Fact]
    public async Task Marcar_TarefaSemAprovacao_JaNascaNaoAplicavel()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenComum);

        var response = await _client.PostAsJsonAsync("/api/execucoes",
            new MarcarExecucaoRequest(_aderenciaSemAprovacaoId, StatusExecucao.Feito, 100m));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var execucao = await response.Content.ReadFromJsonAsync<ExecucaoResponse>();
        Assert.Equal(StatusAprovacao.NaoAplicavel, execucao!.StatusAprovacao);
        Assert.Equal(5m, execucao.ValorCalculado);
    }

    [Fact]
    public async Task Rejeitar_ImpedeAprovacaoSubsequente()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenComum);
        var marcarResponse = await _client.PostAsJsonAsync("/api/execucoes",
            new MarcarExecucaoRequest(_aderenciaComAprovacaoId, StatusExecucao.Feito, 100m));
        var execucao = await marcarResponse.Content.ReadFromJsonAsync<ExecucaoResponse>();

        var tokenMaster = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenMaster);

        var rejeitarResponse = await _client.PostAsync($"/api/execucoes/{execucao!.Id}/rejeitar", null);
        Assert.Equal(HttpStatusCode.OK, rejeitarResponse.StatusCode);
        var rejeitada = await rejeitarResponse.Content.ReadFromJsonAsync<ExecucaoResponse>();
        Assert.Equal(StatusAprovacao.Rejeitado, rejeitada!.StatusAprovacao);

        var aprovarResponse = await _client.PostAsync($"/api/execucoes/{execucao.Id}/aprovar", null);
        Assert.Equal(HttpStatusCode.BadRequest, aprovarResponse.StatusCode);
    }

    [Fact]
    public async Task Marcar_AderenciaDeOutroFilho_Retorna400()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenOutroComum);

        var response = await _client.PostAsJsonAsync("/api/execucoes",
            new MarcarExecucaoRequest(_aderenciaComAprovacaoId, StatusExecucao.Feito, 100m));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Marcar_ParcialEmTarefaQueNaoPermite_Retorna400()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenComum);

        var response = await _client.PostAsJsonAsync("/api/execucoes",
            new MarcarExecucaoRequest(_aderenciaSemParcialId, StatusExecucao.Parcial, 50m));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Comum_NaoConsegueAprovarExecucao()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenComum);
        var response = await _client.PostAsync($"/api/execucoes/{Guid.NewGuid()}/aprovar", null);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task MinhasTarefas_ListaAsAderenciasAtivasDoFilhoAutenticado()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenComum);

        var response = await _client.GetAsync("/api/tarefas/minhas");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var minhasTarefas = await response.Content.ReadFromJsonAsync<List<MinhaTarefaResponse>>();

        Assert.NotNull(minhasTarefas);
        Assert.Equal(3, minhasTarefas!.Count);
    }

    private async Task<string> LoginMasterAAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/master/login", new { email = _emailMasterA, senha = SenhaMasterA });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return body!.Token;
    }
}
