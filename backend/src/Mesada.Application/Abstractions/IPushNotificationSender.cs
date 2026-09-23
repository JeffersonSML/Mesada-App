namespace Mesada.Application.Abstractions;

/// <summary>
/// Abstrai o provedor de push (docs/especificacao.md #notificações —
/// Firebase Cloud Messaging). Implementação concreta em
/// Mesada.Infrastructure.Notifications.FcmPushNotificationSender.
/// </summary>
public interface IPushNotificationSender
{
    /// <param name="tokenDispositivo">Token FCM do aparelho, obtido pelo app mobile no primeiro uso e associado ao UsuarioComum/UsuarioMaster.</param>
    Task EnviarAsync(string tokenDispositivo, string titulo, string corpo, IDictionary<string, string>? dados = null, CancellationToken ct = default);
}
