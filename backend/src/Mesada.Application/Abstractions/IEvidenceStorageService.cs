namespace Mesada.Application.Abstractions;

/// <summary>
/// Abstrai o storage de evidências (docs/especificacao.md — compatível com
/// S3). Implementação concreta usa o AWS SDK apontado para qualquer
/// endpoint S3-compatível via configuração (AWS S3 real, MinIO, etc. —
/// ver Mesada.Infrastructure.Storage.S3EvidenceStorageService).
/// </summary>
public interface IEvidenceStorageService
{
    /// <returns>A storage_key persistida em Evidencia.StorageKey — não uma URL pública.</returns>
    Task<string> EnviarAsync(Guid familiaId, Guid execucaoId, Stream conteudo, string contentType, CancellationToken ct = default);

    /// <summary>URL assinada e temporária para exibir a evidência — nunca um link público permanente.</summary>
    Task<Uri> ObterUrlTemporariaAsync(string storageKey, TimeSpan validade, CancellationToken ct = default);
}
