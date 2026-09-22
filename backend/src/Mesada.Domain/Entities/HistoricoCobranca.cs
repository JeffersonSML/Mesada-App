using Mesada.Domain.Enums;

namespace Mesada.Domain.Entities;

public class HistoricoCobranca
{
    public Guid Id { get; set; }
    public Guid FamiliaId { get; set; }
    public Guid AssinaturaId { get; set; }
    public decimal Valor { get; set; }
    public StatusCobranca Status { get; set; } = StatusCobranca.Pendente;
    public string? ProviderChargeId { get; set; }
    public DateTimeOffset DataCobranca { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Assinatura? Assinatura { get; set; }
}
