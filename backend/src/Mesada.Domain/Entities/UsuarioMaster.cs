namespace Mesada.Domain.Entities;

public class UsuarioMaster
{
    public Guid Id { get; set; }
    public Guid FamiliaId { get; set; }
    public string Nome { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string SenhaHash { get; set; } = default!;
    public bool IsFinanceiro { get; set; }
    public string Status { get; set; } = "ativo";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Familia? Familia { get; set; }
}
