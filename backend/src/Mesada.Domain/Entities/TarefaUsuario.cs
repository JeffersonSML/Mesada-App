namespace Mesada.Domain.Entities;

/// <summary>Aderência N:N entre Tarefa e UsuarioComum.</summary>
public class TarefaUsuario
{
    public Guid Id { get; set; }
    public Guid FamiliaId { get; set; }
    public Guid TarefaId { get; set; }
    public Guid UsuarioComumId { get; set; }
    public decimal? ValorOverride { get; set; }
    public decimal? PontosOverride { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Tarefa? Tarefa { get; set; }
    public UsuarioComum? UsuarioComum { get; set; }
    public ICollection<Execucao> Execucoes { get; set; } = new List<Execucao>();
}
