namespace Mesada.Infrastructure.Notifications;

public sealed class FcmOptions
{
    public const string SecaoConfiguracao = "Fcm";

    /// <summary>Caminho do arquivo JSON da service account do Firebase — nunca versionado.</summary>
    public string CaminhoServiceAccount { get; set; } = default!;
}
