using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Microsoft.Extensions.Options;

namespace Mesada.Infrastructure.Notifications;

/// <summary>
/// Implementação de IPushNotificationSender sobre o SDK oficial
/// "FirebaseAdmin" (NuGet). NÃO validado nesta sessão contra um projeto
/// Firebase real — inicializar o SDK exige uma service account de verdade,
/// que não temos neste ambiente. A montagem da mensagem (a parte sem
/// dependência de credencial) está isolada em ConstruirMensagem, testável
/// sem Firebase nenhum — ver
/// tests/Mesada.Domain.Tests (ou equivalente) para essa cobertura.
/// </summary>
public sealed class FcmPushNotificationSender : IPushNotificationSender
{
    private static readonly object TrancaInicializacao = new();

    public FcmPushNotificationSender(IOptions<FcmOptions> options)
    {
        GarantirAppInicializado(options.Value);
    }

    public async Task EnviarAsync(
        string tokenDispositivo,
        string titulo,
        string corpo,
        IDictionary<string, string>? dados = null,
        CancellationToken ct = default)
    {
        var mensagem = ConstruirMensagem(tokenDispositivo, titulo, corpo, dados);

        try
        {
            await FirebaseMessaging.DefaultInstance.SendAsync(mensagem, ct);
        }
        catch (Exception ex)
        {
            throw new PushNotificationException($"Falha ao enviar push via FCM: {ex.Message}", ex);
        }
    }

    /// <summary>Extraído à parte por ser a única porção desta classe testável sem uma service account real.</summary>
    internal static Message ConstruirMensagem(string tokenDispositivo, string titulo, string corpo, IDictionary<string, string>? dados) => new()
    {
        Token = tokenDispositivo,
        Notification = new Notification { Title = titulo, Body = corpo },
        // Construção explícita (não um `as` cast): IDictionary<K,V> não implica
        // IReadOnlyDictionary<K,V> na interface estática, então um cast direto
        // dependeria do tipo concreto passado pelo chamador e poderia
        // silenciosamente virar null, descartando os dados sem erro nenhum.
        Data = dados is null ? null : new Dictionary<string, string>(dados),
    };

    private static void GarantirAppInicializado(FcmOptions options)
    {
        if (FirebaseApp.DefaultInstance is not null) return;

        lock (TrancaInicializacao)
        {
            if (FirebaseApp.DefaultInstance is not null) return;

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(options.CaminhoServiceAccount),
            });
        }
    }
}
