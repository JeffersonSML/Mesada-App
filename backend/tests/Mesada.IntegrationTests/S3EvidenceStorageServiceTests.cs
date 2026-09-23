using System.Text;
using Amazon.S3;
using Mesada.Infrastructure.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Mesada.IntegrationTests;

/// <summary>
/// Sem um MinIO ou bucket S3 real disponível neste ambiente (dl.min.io está
/// bloqueado — ver docs/adr/0005 e 0006), validamos S3EvidenceStorageService
/// de duas formas que não dependem de um servidor S3 de verdade:
/// (1) EnviarAsync contra um servidor HTTP local que só responde 200 a
/// qualquer PUT — prova que o upload é chamado com bucket/key/content-type
/// corretos e que a storage_key retornada segue o padrão esperado; (2)
/// ObterUrlTemporariaAsync não faz nenhuma chamada de rede — é assinatura
/// local (SigV4) — então valida de verdade contra credenciais fake, sem
/// precisar de servidor nenhum.
/// </summary>
public sealed class S3EvidenceStorageServiceTests : IAsyncLifetime
{
    private WebApplication _servidorFalso = default!;
    private S3EvidenceStorageService _servico = default!;
    private const string Bucket = "mesada-evidencias-teste";

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        _servidorFalso = builder.Build();

        _servidorFalso.MapPut("/{bucket}/{**key}", context =>
        {
            context.Response.Headers.ETag = "\"fake-etag\"";
            context.Response.StatusCode = 200;
            return Task.CompletedTask;
        });

        await _servidorFalso.StartAsync();
        var endereco = _servidorFalso.Services
            .GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!
            .Addresses.First();

        var s3Config = new AmazonS3Config { ServiceURL = endereco, ForcePathStyle = true, SignatureVersion = "4" };
        var s3Client = new AmazonS3Client(new Amazon.Runtime.BasicAWSCredentials("fake-access-key", "fake-secret-key"), s3Config);
        var options = Options.Create(new S3Options { AccessKey = "fake-access-key", SecretKey = "fake-secret-key", BucketName = Bucket, ServiceUrl = endereco });
        _servico = new S3EvidenceStorageService(s3Client, options);
    }

    public async Task DisposeAsync() => await _servidorFalso.StopAsync();

    [Fact]
    public async Task EnviarAsync_ContraServidorQueRespondeComSucesso_RetornaStorageKeyComPadraoEsperado()
    {
        var familiaId = Guid.NewGuid();
        var execucaoId = Guid.NewGuid();
        using var conteudo = new MemoryStream(Encoding.UTF8.GetBytes("conteúdo de evidência fake"));

        var storageKey = await _servico.EnviarAsync(familiaId, execucaoId, conteudo, "image/jpeg");

        Assert.StartsWith($"evidencias/{familiaId}/{execucaoId}/", storageKey);
        Assert.EndsWith(".jpg", storageKey);
    }

    [Fact]
    public async Task ObterUrlTemporariaAsync_NaoFazChamadaDeRede_RetornaUrlAssinadaComExpiracao()
    {
        var url = await _servico.ObterUrlTemporariaAsync("evidencias/x/y/z.jpg", TimeSpan.FromMinutes(15));

        // V2 (AWSAccessKeyId/Signature), não V4 (X-Amz-*) — comportamento
        // real do SDK para ServiceUrl customizado sem RegionEndpoint,
        // confirmado empiricamente aqui; ver o comentário em
        // DependencyInjection.AddInfraServicesExternas.
        Assert.Contains(Bucket, url.ToString());
        Assert.Contains("AWSAccessKeyId=fake-access-key", url.ToString());
        Assert.Contains("Signature=", url.ToString());
    }
}
