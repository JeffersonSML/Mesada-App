using Mesada.Infrastructure.Notifications;
using Xunit;

namespace Mesada.IntegrationTests;

/// <summary>
/// FcmPushNotificationSender.ConstruirMensagem é a única parte do adapter
/// de push testável sem uma service account real do Firebase — o resto
/// (FirebaseApp.Create, FirebaseMessaging.SendAsync) exige credenciais que
/// não temos neste ambiente (ver docs/adr/0006-*.md).
/// </summary>
public sealed class FcmPushNotificationSenderTests
{
    [Fact]
    public void ConstruirMensagem_PreencheTokenTituloECorpo()
    {
        var mensagem = FcmPushNotificationSender.ConstruirMensagem("token-abc", "Tarefa aprovada", "Sua tarefa foi aprovada!", null);

        Assert.Equal("token-abc", mensagem.Token);
        Assert.Equal("Tarefa aprovada", mensagem.Notification.Title);
        Assert.Equal("Sua tarefa foi aprovada!", mensagem.Notification.Body);
        Assert.Null(mensagem.Data);
    }

    [Fact]
    public void ConstruirMensagem_CopiaDadosExtrasSemReferenciarADictionaryOriginal()
    {
        var dadosOriginais = new Dictionary<string, string> { ["execucaoId"] = "123" };

        var mensagem = FcmPushNotificationSender.ConstruirMensagem("token-abc", "Título", "Corpo", dadosOriginais);
        dadosOriginais["execucaoId"] = "999"; // não deve afetar a mensagem já construída

        Assert.Equal("123", mensagem.Data!["execucaoId"]);
    }
}
