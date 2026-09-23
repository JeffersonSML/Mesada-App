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

/// <summary>Prova o cadastro de tarefas, aderências e a sugestão automática de valor/pontos de ponta a ponta.</summary>
public sealed class TarefasEndToEndTests : IClassFixture<MesadaWebApplicationFactory>, IAsyncLifetime
{
    private const string SenhaMasterA = "Teste@123";

    private readonly MesadaWebApplicationFactory _factory;
    private HttpClient _client = default!;

    private Guid _familiaAId;
    private Guid _categoriaId;
    private Guid _filhoValorDiretoId;
    private Guid _filhoPontosId;
    private string _emailMasterA = default!;

    public TarefasEndToEndTests(MesadaWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var familiaA = new Familia { Id = Guid.NewGuid(), Nome = "[teste-e2e] Família A - Tarefas" };
        admin.Familias.Add(familiaA);
        // Categoria não tem navegação para Familia no modelo EF (só FamiliaId
        // cru), então o SaveChanges não consegue inferir a ordem de inserção
        // sozinho — precisa da família já persistida antes.
        await admin.SaveChangesAsync();

        _emailMasterA = $"master-tarefas-{Guid.NewGuid():N}@teste.local";
        admin.UsuariosMaster.Add(new UsuarioMaster
        {
            Id = Guid.NewGuid(),
            FamiliaId = familiaA.Id,
            Nome = "Master A",
            Email = _emailMasterA,
            SenhaHash = hasher.Hash(SenhaMasterA),
            IsFinanceiro = true,
        });

        var filhoValorDireto = new UsuarioComum { Id = Guid.NewGuid(), FamiliaId = familiaA.Id, Nome = "[teste-e2e] Filho Valor Direto", MesadaBase = 100m };
        var filhoPontos = new UsuarioComum { Id = Guid.NewGuid(), FamiliaId = familiaA.Id, Nome = "[teste-e2e] Filho Pontos", MesadaBase = 100m, ValorPonto = 2m };
        admin.UsuariosComuns.AddRange(filhoValorDireto, filhoPontos);

        var categoria = new Categoria { Id = Guid.NewGuid(), FamiliaId = familiaA.Id, Nome = "[teste-e2e] Categoria" };
        admin.Categorias.Add(categoria);

        await admin.SaveChangesAsync();

        _familiaAId = familiaA.Id;
        _categoriaId = categoria.Id;
        _filhoValorDiretoId = filhoValorDireto.Id;
        _filhoPontosId = filhoPontos.Id;
    }

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        await admin.Familias.Where(f => f.Id == _familiaAId).ExecuteDeleteAsync();
    }

    private static SalvarTarefaRequest TarefaValorDiretoValida(Guid categoriaId) => new(
        categoriaId, "Arrumar a cama", null, ModoCalculo.ValorDireto, 10m, null, false,
        TipoTarefa.Recorrente, null, NaturezaTarefa.Bonus, null, true, TipoEvidencia.Foto, null);

    [Fact]
    public async Task CriarAtualizarERemover_Tarefa_FuncionaDePontaAPonta()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var criarResponse = await _client.PostAsJsonAsync("/api/tarefas", TarefaValorDiretoValida(_categoriaId));
        Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);
        var criada = await criarResponse.Content.ReadFromJsonAsync<TarefaResponse>();
        Assert.Equal(10m, criada!.Valor);

        var atualizarRequest = TarefaValorDiretoValida(_categoriaId) with { Nome = "Arrumar a cama - renomeada", Valor = 15m };
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/tarefas/{criada.Id}", atualizarRequest);
        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);
        var atualizada = await atualizarResponse.Content.ReadFromJsonAsync<TarefaResponse>();
        Assert.Equal("Arrumar a cama - renomeada", atualizada!.Nome);
        Assert.Equal(15m, atualizada.Valor);

        var removerResponse = await _client.DeleteAsync($"/api/tarefas/{criada.Id}");
        Assert.Equal(HttpStatusCode.NoContent, removerResponse.StatusCode);

        var listaResponse = await _client.GetAsync("/api/tarefas");
        var lista = await listaResponse.Content.ReadFromJsonAsync<List<TarefaResponse>>();
        Assert.DoesNotContain(lista!, t => t.Id == criada.Id);
    }

    [Fact]
    public async Task Criar_ValorDiretoSemValor_Retorna400()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = TarefaValorDiretoValida(_categoriaId) with { Valor = null };
        var response = await _client.PostAsJsonAsync("/api/tarefas", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Criar_AvulsaSemValidade_Retorna400()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = TarefaValorDiretoValida(_categoriaId) with { Tipo = TipoTarefa.Avulsa, ValidadeAvulsa = null };
        var response = await _client.PostAsJsonAsync("/api/tarefas", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Criar_MultaEmTarefaBonus_Retorna400()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = TarefaValorDiretoValida(_categoriaId) with { Natureza = NaturezaTarefa.Bonus, ValorMulta = 5m };
        var response = await _client.PostAsJsonAsync("/api/tarefas", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AdicionarAderencia_DuasVezesParaOMesmoFilho_RetornaConflitoNaSegunda()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var tarefaResponse = await _client.PostAsJsonAsync("/api/tarefas", TarefaValorDiretoValida(_categoriaId));
        var tarefa = await tarefaResponse.Content.ReadFromJsonAsync<TarefaResponse>();

        var primeira = await _client.PostAsJsonAsync($"/api/tarefas/{tarefa!.Id}/aderencias",
            new AderenciaRequest(_filhoValorDiretoId, null, null));
        Assert.Equal(HttpStatusCode.Created, primeira.StatusCode);

        var segunda = await _client.PostAsJsonAsync($"/api/tarefas/{tarefa.Id}/aderencias",
            new AderenciaRequest(_filhoValorDiretoId, null, null));
        Assert.Equal(HttpStatusCode.BadRequest, segunda.StatusCode);

        var removerResponse = await _client.DeleteAsync($"/api/tarefas/{tarefa.Id}/aderencias/{_filhoValorDiretoId}");
        Assert.Equal(HttpStatusCode.NoContent, removerResponse.StatusCode);

        // Readerir após remoção reativa a linha em vez de duplicar (UNIQUE tarefa_id/usuario_comum_id).
        var terceira = await _client.PostAsJsonAsync($"/api/tarefas/{tarefa.Id}/aderencias",
            new AderenciaRequest(_filhoValorDiretoId, 99m, null));
        Assert.Equal(HttpStatusCode.Created, terceira.StatusCode);
    }

    [Fact]
    public async Task Sugerir_ValorDireto_DivideMesadaBasePelaQuantidadeDeAderentesMaisUm()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Sem nenhuma aderência ainda: mesada_base (100) / (0 + 1) = 100.
        var response = await _client.GetAsync($"/api/tarefas/sugestao?usuarioComumId={_filhoValorDiretoId}&modoCalculo=ValorDireto");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sugestao = await response.Content.ReadFromJsonAsync<SugestaoValorResponse>();
        Assert.Equal(100m, sugestao!.Valor);

        var tarefaResponse = await _client.PostAsJsonAsync("/api/tarefas", TarefaValorDiretoValida(_categoriaId));
        var tarefa = await tarefaResponse.Content.ReadFromJsonAsync<TarefaResponse>();
        await _client.PostAsJsonAsync($"/api/tarefas/{tarefa!.Id}/aderencias", new AderenciaRequest(_filhoValorDiretoId, null, null));

        // Com 1 aderência: mesada_base (100) / (1 + 1) = 50.
        var segundaResponse = await _client.GetAsync($"/api/tarefas/sugestao?usuarioComumId={_filhoValorDiretoId}&modoCalculo=ValorDireto");
        var segundaSugestao = await segundaResponse.Content.ReadFromJsonAsync<SugestaoValorResponse>();
        Assert.Equal(50m, segundaSugestao!.Valor);
    }

    [Fact]
    public async Task Sugerir_Pontos_UsaValorPontoDoFilho()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Pontos_totais = 100 / 2 = 50; Pontos_sugeridos = 50 / 1 = 50.
        var response = await _client.GetAsync($"/api/tarefas/sugestao?usuarioComumId={_filhoPontosId}&modoCalculo=Pontos");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sugestao = await response.Content.ReadFromJsonAsync<SugestaoValorResponse>();
        Assert.Equal(50m, sugestao!.Valor);
    }

    [Fact]
    public async Task Sugerir_PontosSemValorPontoConfigurado_Retorna400()
    {
        var token = await LoginMasterAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/tarefas/sugestao?usuarioComumId={_filhoValorDiretoId}&modoCalculo=Pontos");
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
