namespace Mesada.Domain.Entities;

public class Categoria
{
    public Guid Id { get; set; }
    public Guid? FamiliaId { get; set; }
    public Guid? ParentId { get; set; }
    public string Nome { get; set; } = default!;
    public bool Sistema { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Categoria? Parent { get; set; }
    public ICollection<Categoria> Subcategorias { get; set; } = new List<Categoria>();
}
