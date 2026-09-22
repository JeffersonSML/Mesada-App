using Mesada.Domain.Enums;

namespace Mesada.Domain.Entities;

public class Execucao
{
    public Guid Id { get; set; }
    public Guid FamiliaId { get; set; }
    public Guid TarefaUsuarioId { get; set; }
    public Guid? CicloMesadaId { get; set; }
    public DateTimeOffset DataExecucao { get; set; }
    public StatusExecucao Status { get; set; } = StatusExecucao.Pendente;
    public decimal PercentualConclusao { get; set; } = 100m;
    public StatusAprovacao StatusAprovacao { get; set; } = StatusAprovacao.NaoAplicavel;
    public Guid? AprovadoPor { get; set; }
    public DateTimeOffset? AprovadoEm { get; set; }
    public decimal? ValorCalculado { get; set; }
    public decimal? PontosCalculado { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public TarefaUsuario? TarefaUsuario { get; set; }
    public CicloMesada? CicloMesada { get; set; }
    public ICollection<Evidencia> Evidencias { get; set; } = new List<Evidencia>();
}
