using Mesada.Domain.Enums;

namespace Mesada.Domain.Entities;

public class Familia
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = default!;
    public CicloPeriodicidade CicloFechamentoPadrao { get; set; } = CicloPeriodicidade.Mensal;
    public StatusFamilia Status { get; set; } = StatusFamilia.Ativa;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<UsuarioMaster> Masters { get; set; } = new List<UsuarioMaster>();
    public ICollection<UsuarioComum> Filhos { get; set; } = new List<UsuarioComum>();
}
