using Mesada.Domain.Enums;

namespace Mesada.Domain.Entities;

public class NotificacaoConfig
{
    public Guid Id { get; set; }
    public Guid FamiliaId { get; set; }
    public EventoNotificacao Evento { get; set; }
    public CanalNotificacao Canal { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
