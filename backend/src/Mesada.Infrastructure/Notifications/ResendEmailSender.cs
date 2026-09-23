using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Microsoft.Extensions.Options;
using Resend;

namespace Mesada.Infrastructure.Notifications;

public sealed class EmailOptions
{
    public const string SecaoConfiguracao = "Email";

    public string RemetenteEmail { get; set; } = "nao-responda@mesadaapp.com.br";
    public string RemetenteNome { get; set; } = "Mesada App";
}

/// <summary>
/// Implementação de IEmailSender sobre o SDK oficial "Resend" (NuGet).
/// Registrado via services.AddResend(apiKey) — ver DependencyInjection.cs.
/// Validado nesta sessão contra um servidor HTTP local que imita o
/// contrato da API do Resend (sem chave real de produção/sandbox) — ver
/// tests/Mesada.IntegrationTests/ResendEmailSenderTests.cs.
/// </summary>
public sealed class ResendEmailSender(IResend resend, IOptions<EmailOptions> options) : IEmailSender
{
    public async Task EnviarAsync(string destinatarioEmail, string assunto, string corpoHtml, CancellationToken ct = default)
    {
        var mensagem = new EmailMessage
        {
            From = new EmailAddress { Email = options.Value.RemetenteEmail, DisplayName = options.Value.RemetenteNome },
            To = EmailAddressList.From([destinatarioEmail]),
            Subject = assunto,
            HtmlBody = corpoHtml,
        };

        var resposta = await resend.EmailSendAsync(mensagem, ct);
        if (!resposta.Success)
            throw new EmailSenderException(resposta.Exception?.Message ?? "Erro desconhecido retornado pelo Resend.", resposta.Exception);
    }
}
