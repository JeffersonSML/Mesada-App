using Mesada.Domain.Enums;

namespace Mesada.Domain.Entities;

public class Assinatura
{
    public Guid Id { get; set; }
    public Guid FamiliaId { get; set; }
    public Guid PlanoId { get; set; }
    public StatusAssinatura Status { get; set; } = StatusAssinatura.Trial;
    public string? Provider { get; set; }
    public string? ProviderCustomerId { get; set; }
    public string? ProviderSubscriptionId { get; set; }
    public DateTimeOffset DataInicio { get; set; }
    public DateTimeOffset? DataFim { get; set; }
    public DateTimeOffset? ProximaCobranca { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Plano? Plano { get; set; }
    public ICollection<HistoricoCobranca> HistoricoCobrancas { get; set; } = new List<HistoricoCobranca>();
}
