namespace Mesada.Domain.Entities;

/// <summary>
/// Grupo de acesso do painel administrativo interno — não confundir com
/// papéis de família (Master/Comum). <see cref="Sistema"/> = true marca o
/// grupo Owner (acesso total, único, protegido contra edição/remoção pela
/// API); os demais grupos (ex.: "Tecnologia") são livremente geridos pelo
/// Owner e suas permissões ficam em <see cref="Permissoes"/> (jsonb bruto).
/// </summary>
public class GrupoAdministrador
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = default!;
    public string? Descricao { get; set; }
    public string Permissoes { get; set; } = "{}"; // jsonb bruto
    public bool Sistema { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
