using Mesada.Domain.Enums;

namespace Mesada.Domain.Entities;

public class Plano
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = default!;
    public string Nome { get; set; } = default!;
    public decimal PrecoMensal { get; set; }
    public int? LimiteFilhos { get; set; }
    public int? LimiteMastersAdicionais { get; set; }
    public bool PermiteCategoriasCustomizadas { get; set; }
    public bool PermiteIntegracoesExternas { get; set; }
    public NivelRelatorio NivelRelatorios { get; set; } = NivelRelatorio.ExtratoBasico;
    public NivelSuporte NivelSuporte { get; set; } = NivelSuporte.SelfService;
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
