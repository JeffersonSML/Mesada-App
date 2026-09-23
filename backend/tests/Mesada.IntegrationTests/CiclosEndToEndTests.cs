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
/// Prova o fechamento de ciclo de ponta a ponta (docs/especificacao.md
/// #ciclo-de-mesada): Mesada_final = Mesada_base + Σbônus − Σmultas,
/// consolidando execuções reais marcadas via API.
/// </summary>
public sealed class CiclosEndToEndTests : IClassFixture<MesadaWebApplicationFactory>, IAsyncLifetime
{
    private const string SenhaMasterA = "Teste@123";
    private const decimal MesadaBase = 100m;
    private const decimal ValorTarefaBonus = 10m;
    private const decimal MultaTarefaObrigatoria = 5m;

    private readonly MesadaWebApplicationFactory _factory;
    private HttpClient _client = default!;

    private Guid _familiaAId;
    private Guid _filhoId;
    private string _emailMasterA = default!;
    private string _tokenComum = default!;

    public CiclosEndToEndTests(MesadaWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var tokens = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        var familiaA = new Familia { Id = Guid.NewGuid(), Nome = "[teste-e2e] Família A - Ciclos" };
        admin.Familias.Add(familiaA);
        await admin.SaveChangesAsync();

        _emailMasterA = $"master-ciclos-{Guid.NewGuid():N}@teste.local";
        admin.UsuariosMaster.Add(new UsuarioMaster
        {
            Id = Guid.NewGuid(),
            FamiliaId = familiaA.Id,
            Nome = "Master A",
            Email = _emailMasterA,
            SenhaHash = hasher.Hash(SenhaMasterA),
            IsFinanceiro = true,
        });

        var filho = new UsuarioComum { Id = Guid.NewGuid(), FamiliaId = familiaA.Id, Nome = "[teste-e2e] Filho Ciclo", MesadaBase = MesadaBase };
        admin.UsuariosComuns.Add(filho);

        var categoria = new Categoria { Id = Guid.NewGuid(), FamiliaId = familiaA.Id, Nome = "[teste-e2e] Categoria Ciclos" };
        admin.Categorias.Add(categoria);

        var tarefaBonus = new Tarefa
        {
            Id = Guid.NewGuid(), FamiliaId = familiaA.Id, CategoriaId = categoria.Id, Nome = "Tarefa Bônus",
            ModoCalculo = ModoCalculo.ValorDireto, Valor = ValorTarefaBonus, Natureza = NaturezaTarefa.Bonus, RequerAprovacao = false,
        };
        var tarefaObrigatoria = new Tarefa
        {
            Id = Guid.NewGuid(), FamiliaId = familiaA.Id, CategoriaId = categoria.Id, Nome = "Tarefa Obrigatória",
            ModoCalculo = ModoCalculo.ValorDireto, Valor = 0m, Natureza = NaturezaTarefa.Obrigatoria,
            ValorMulta = MultaTarefaObrigatoria, RequerAprovacao = false,
        };
        admin.Tarefas.AddRange(tarefaBonus, tarefaObrigatoria);
        await admin.SaveChangesAsync();

        var aderenciaBonus = new TarefaUsuario { Id = Guid.NewGuid(), FamiliaId = familiaA.Id, TarefaId = tarefaBonus.Id, UsuarioComumId = filho.Id };
        var aderenciaObrigatoria = new TarefaUsuario { Id = Guid.NewGuid(), FamiliaId = familiaA.Id, TarefaId = tarefaObrigatoria.Id, UsuarioComumId = filho.Id };
        admin.TarefasUsuarios.AddRange(aderenciaBonus, aderenciaObrigatoria);
        await admin.SaveChangesAsync();

        _familiaAId = familiaA.Id;
        _filhoId = filho.Id;
        _tokenComum = tokens.GerarTokenComum(filho, "dispositivo-teste-ciclo");

        // Marca as execuções que o fechamento vai consolidar: bônus cumprido, obrigatória não cumprida (gera multa).
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenComum);
        await _client.PostAsJsonAsync("/api/execucoes", new MarcarExecucaoRequest(aderenciaBonus.Id, StatusExecucao.Feito, 100m));
        await _client.PostAsJsonAsync("/api/execucoes", new MarcarExecucaoRequest(aderenciaObrigatoria.Id, StatusExecucao.NaoFeito, 0m));
        _client.DefaultRequestHeaders.Authorization = null;
    }

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        await admin.Familias.Where(f => f.Id == _familiaAId).ExecuteDeleteAsync();
    }

    [Fact]
    public async Task Fechar_ConsolidaBonusEMultaSegundoAFormula()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var response = await _client.PostAsJsonAsync("/api/ciclos/fechar", new FecharCicloRequest(_filhoId, hoje, hoje));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var ciclo = await response.Content.ReadFromJsonAsync<CicloResponse>();

        Assert.Equal(MesadaBase, ciclo!.MesadaBase);
        Assert.Equal(ValorTarefaBonus, ciclo.SomaBonus);
        Assert.Equal(MultaTarefaObrigatoria, ciclo.SomaMultas);
        Assert.Equal(0m, ciclo.SaldoDevedorAnterior);
        // Mesada_final = 100 + 10 - 5 = 105 (>= 0, sem débito).
        Assert.Equal(105m, ciclo.ValorFinal);
        Assert.Equal(0m, ciclo.SaldoDevedorResultante);
    }

    [Fact]
    public async Task Fechar_DuasVezesComMesmaDataInicio_Retorna400()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var primeira = await _client.PostAsJsonAsync("/api/ciclos/fechar", new FecharCicloRequest(_filhoId, hoje, hoje));
        Assert.Equal(HttpStatusCode.Created, primeira.StatusCode);

        var segunda = await _client.PostAsJsonAsync("/api/ciclos/fechar", new FecharCicloRequest(_filhoId, hoje, hoje));
        Assert.Equal(HttpStatusCode.BadRequest, segunda.StatusCode);
    }

    [Fact]
    public async Task Fechar_ComDataFimAnteriorADataInicio_Retorna400()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var response = await _client.PostAsJsonAsync("/api/ciclos/fechar", new FecharCicloRequest(_filhoId, hoje, hoje.AddDays(-1)));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Fechar_DeFilhoInexistente_Retorna404()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var response = await _client.PostAsJsonAsync("/api/ciclos/fechar", new FecharCicloRequest(Guid.NewGuid(), hoje, hoje));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FecharEConsultarHistorico_TrazOCicloRecemFechado()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        await _client.PostAsJsonAsync("/api/ciclos/fechar", new FecharCicloRequest(_filhoId, hoje, hoje));

        var historicoResponse = await _client.GetAsync($"/api/ciclos/historico?usuarioComumId={_filhoId}");
        Assert.Equal(HttpStatusCode.OK, historicoResponse.StatusCode);
        var historico = await historicoResponse.Content.ReadFromJsonAsync<List<CicloResponse>>();

        Assert.NotNull(historico);
        Assert.Single(historico!);
        Assert.Equal(105m, historico![0].ValorFinal);
    }

    [Fact]
    public async Task Atual_AntesDeQualquerFechamento_RefleteAsExecucoesJaMarcadas()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/ciclos/atual?usuarioComumId={_filhoId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var atual = await response.Content.ReadFromJsonAsync<CicloAtualResponse>();

        Assert.Equal(ValorTarefaBonus, atual!.SomaBonus);
        Assert.Equal(MultaTarefaObrigatoria, atual.SomaMultas);
        Assert.Equal(105m, atual.ValorFinalPrevisto);
    }

    private async Task<string> LoginMasterAAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/master/login", new { email = _emailMasterA, senha = SenhaMasterA });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return body!.Token;
    }
}
