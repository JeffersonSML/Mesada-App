using Amazon.S3;
using Amazon.S3.Model;
using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;

namespace Mesada.Infrastructure.Storage;

/// <summary>
/// Implementação de IEvidenceStorageService sobre o AWS SDK
/// (AWSSDK.S3), apontável para qualquer endpoint S3-compatível via
/// S3Options.ServiceUrl (MinIO em desenvolvimento, AWS S3 real em
/// produção) — nunca acopla o resto do sistema à AWS especificamente.
/// A chave de objeto segue familia/execucao/guid.ext para nunca colidir
/// entre tenants e nunca expor um caminho previsível.
/// </summary>
public sealed class S3EvidenceStorageService(IAmazonS3 s3Client, Microsoft.Extensions.Options.IOptions<S3Options> options) : IEvidenceStorageService
{
    private readonly S3Options _options = options.Value;

    public async Task<string> EnviarAsync(Guid familiaId, Guid execucaoId, Stream conteudo, string contentType, CancellationToken ct = default)
    {
        var extensao = contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            _ => "",
        };
        var storageKey = $"evidencias/{familiaId}/{execucaoId}/{Guid.NewGuid()}{extensao}";

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = storageKey,
            InputStream = conteudo,
            ContentType = contentType,
            AutoCloseStream = false,
        };

        try
        {
            await s3Client.PutObjectAsync(request, ct);
        }
        catch (AmazonS3Exception ex)
        {
            throw new EvidenceStorageException($"Falha ao enviar evidência para o storage: {ex.Message}", ex);
        }

        return storageKey;
    }

    public async Task<Uri> ObterUrlTemporariaAsync(string storageKey, TimeSpan validade, CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = storageKey,
            Expires = DateTime.UtcNow.Add(validade),
            Verb = HttpVerb.GET,
        };

        var url = await s3Client.GetPreSignedURLAsync(request);
        return new Uri(url);
    }
}
