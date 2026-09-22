using Mesada.Domain.Enums;

namespace Mesada.Domain.Entities;

public class CicloMesada
{
    public Guid Id { get; set; }
    public Guid FamiliaId { get; set; }
    public Guid UsuarioComumId { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
    public decimal MesadaBase { get; set; }
    public decimal SomaBonus { get; set; }
    public decimal SomaMultas { get; set; }
    public decimal SaldoDevedorAnterior { get; set; }
    public decimal? ValorFinal { get; set; }
    public decimal SaldoDevedorResultante { get; set; }
    public StatusCiclo Status { get; set; } = StatusCiclo.Aberto;
    public DateTimeOffset? FechadoEm { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public UsuarioComum? UsuarioComum { get; set; }
    public ICollection<Execucao> Execucoes { get; set; } = new List<Execucao>();
}
