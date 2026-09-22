using Mesada.Domain.Enums;

namespace Mesada.Domain.Entities;

public class UsuarioComum
{
    public Guid Id { get; set; }
    public Guid FamiliaId { get; set; }
    public string Nome { get; set; } = default!;
    public string? Apelido { get; set; }
    public CicloPeriodicidade CicloFechamento { get; set; } = CicloPeriodicidade.Mensal;
    public decimal SaldoDevedorAcumulado { get; set; }
    public string? DispositivoVinculado { get; set; }
    public string Status { get; set; } = "ativo";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Familia? Familia { get; set; }
}
