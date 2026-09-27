using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mesada.IntegrationTests;

/// <summary>
/// O HttpClient de teste, por padrão, serializa/desserializa enums como
/// número — só o servidor (Program.cs) está configurado com
/// JsonStringEnumConverter. Sem isso aqui, PostAsJsonAsync/ReadFromJsonAsync
/// enviariam/esperariam números onde a API real usa strings (ex.: "Mensal"),
/// mascarando exatamente o bug que só apareceu testando via HTTP puro
/// (curl) fora da suíte de testes.
/// </summary>
internal static class MesadaJsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
