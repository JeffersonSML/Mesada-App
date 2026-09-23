namespace Mesada.Application.Abstractions;

/// <summary>
/// Abstrai o provedor de e-mail transacional (docs/especificacao.md
/// #notificações — SendGrid ou Resend). Implementação concreta hoje usa
/// Resend (Mesada.Infrastructure.Notifications.ResendEmailSender).
/// </summary>
public interface IEmailSender
{
    Task EnviarAsync(string destinatarioEmail, string assunto, string corpoHtml, CancellationToken ct = default);
}
