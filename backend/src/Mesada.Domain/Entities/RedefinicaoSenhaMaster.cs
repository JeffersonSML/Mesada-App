namespace Mesada.Domain.Entities;

public class RedefinicaoSenhaMaster
{
    public Guid Id { get; set; }
    public Guid UsuarioMasterId { get; set; }
    public string TokenHash { get; set; } = default!;
    public DateTimeOffset ExpiraEm { get; set; }
    public DateTimeOffset? UtilizadoEm { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public UsuarioMaster? UsuarioMaster { get; set; }
}
