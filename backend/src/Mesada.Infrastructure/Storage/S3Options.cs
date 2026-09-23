namespace Mesada.Infrastructure.Storage;

public sealed class S3Options
{
    public const string SecaoConfiguracao = "S3";

    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string BucketName { get; set; } = default!;
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// Null/vazio = AWS S3 real. Preenchido = qualquer serviço
    /// S3-compatível (MinIO, DigitalOcean Spaces, Cloudflare R2, etc.),
    /// ex.: "http://127.0.0.1:9000" para um MinIO local.
    /// </summary>
    public string? ServiceUrl { get; set; }
}
