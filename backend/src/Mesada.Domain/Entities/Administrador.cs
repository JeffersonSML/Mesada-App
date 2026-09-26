namespace Mesada.Domain.Entities;

/// <summary>Administrador do sistema — não pertence a nenhuma Familia.</summary>
public class Administrador
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string SenhaHash { get; set; } = default!;
    public string Permissoes { get; set; } = "{}"; // jsonb bruto
    public string Status { get; set; } = "ativo";
    public Guid? GrupoId { get; set; }
    /// <summary>true logo após criação/reset de senha — force a troca antes de liberar qualquer outra ação.</summary>
    public bool DeveTrocarSenha { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public GrupoAdministrador? Grupo { get; set; }
}
