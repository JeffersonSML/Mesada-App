using System.Net;
using System.Net.Http.Json;
using Mesada.Api.Contracts;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Mesada.IntegrationTests;

/// <summary>Prova o signup de ponta a ponta: cria Família + primeiro Master (sempre financeiro) e já devolve um token utilizável.</summary>
public sealed class CriarFamiliaEndToEndTests : IClassFixture<MesadaWebApplicationFactory>
{
    private readonly MesadaWebApplicationFactory _factory;

    public CriarFamiliaEndToEndTests(MesadaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Criar_ComDadosValidos_RetornaTokenUtilizavelNoLogin()
    {
        var client = _factory.CreateClient();
        var email = $"master-signup-{Guid.NewGuid():N}@teste.local";
        Guid? familiaId = null;

        try
        {
            var response = await client.PostAsJsonAsync("/api/familias",
                new CriarFamiliaRequest("[teste-e2e] Família Signup", "Master Signup", email, "SenhaForte@123"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
            Assert.False(string.IsNullOrWhiteSpace(token!.Token));

            var loginResponse = await client.PostAsJsonAsync("/api/auth/master/login", new { email, senha = "SenhaForte@123" });
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
            var master = await admin.UsuariosMaster.SingleAsync(m => m.Email == email);
            Assert.True(master.IsFinanceiro);
            familiaId = master.FamiliaId;
        }
        finally
        {
            if (familiaId is not null)
            {
                using var scope = _factory.Services.CreateScope();
                var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
                await admin.Familias.Where(f => f.Id == familiaId).ExecuteDeleteAsync();
            }
        }
    }

    [Fact]
    public async Task Criar_ComEmailJaCadastrado_Retorna409()
    {
        var client = _factory.CreateClient();
        var email = $"master-signup-dup-{Guid.NewGuid():N}@teste.local";
        var request = new CriarFamiliaRequest("[teste-e2e] Família Dup", "Master Dup", email, "SenhaForte@123");

        var primeira = await client.PostAsJsonAsync("/api/familias", request);
        Assert.Equal(HttpStatusCode.Created, primeira.StatusCode);

        try
        {
            var segunda = await client.PostAsJsonAsync("/api/familias", request);
            Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
        }
        finally
        {
            using var scope = _factory.Services.CreateScope();
            var admin = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
            var master = await admin.UsuariosMaster.SingleAsync(m => m.Email == email);
            await admin.Familias.Where(f => f.Id == master.FamiliaId).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task Criar_ComSenhaCurta_Retorna400()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/familias",
            new CriarFamiliaRequest("[teste-e2e] Família Senha Curta", "Master", $"m-{Guid.NewGuid():N}@teste.local", "123"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
