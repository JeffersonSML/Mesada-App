using Mesada.Domain.Enums;

namespace Mesada.Domain.Entities;

public class Evidencia
{
    public Guid Id { get; set; }
    public Guid FamiliaId { get; set; }
    public Guid ExecucaoId { get; set; }
    public TipoEvidencia Tipo { get; set; }
    public string? StorageKey { get; set; }
    public ProvedorIntegracao? IntegracaoProvider { get; set; }
    public string? IntegracaoDados { get; set; } // jsonb bruto — desserializado pela Application quando necessário
    public DateTimeOffset CreatedAt { get; set; }

    public Execucao? Execucao { get; set; }
}
