using Mesada.Domain.Enums;

namespace Mesada.Domain.Entities;

/// <summary>E-mail ou telefone adicional que recebe as notificações da família, além do cadastro principal do Master/Comum.</summary>
public class NotificacaoDestinatario
{
    public Guid Id { get; set; }
    public Guid FamiliaId { get; set; }
    public TipoDestinatarioNotificacao Tipo { get; set; }
    public string Valor { get; set; } = default!;
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
