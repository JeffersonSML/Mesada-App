using Mesada.Domain.Enums;

namespace Mesada.Domain.Entities;

public class SuporteTicket
{
    public Guid Id { get; set; }
    public Guid FamiliaId { get; set; }
    public Guid UsuarioMasterId { get; set; }
    public string Assunto { get; set; } = default!;
    public string? Descricao { get; set; }
    public StatusTicket Status { get; set; } = StatusTicket.Aberto;
    public string Prioridade { get; set; } = "normal";
    public DateTimeOffset? ResolvidoEm { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
