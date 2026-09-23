namespace Mesada.Application.Exceptions;

/// <summary>Erro retornado pelo storage de evidências (ex.: S3/MinIO) — nunca deixa vazar o tipo/SDK concreto para fora de Infrastructure.</summary>
public sealed class EvidenceStorageException(string mensagem, Exception? innerException = null)
    : Exception(mensagem, innerException);
