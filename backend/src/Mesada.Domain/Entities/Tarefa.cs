using Mesada.Domain.Enums;

namespace Mesada.Domain.Entities;

public class Tarefa
{
    public Guid Id { get; set; }
    public Guid FamiliaId { get; set; }
    public Guid CategoriaId { get; set; }
    public string Nome { get; set; } = default!;
    public string? Descricao { get; set; }
    public ModoCalculo ModoCalculo { get; set; }
    public decimal? Valor { get; set; }
    public decimal? Pontos { get; set; }
    public bool PermiteParcial { get; set; }
    public TipoTarefa Tipo { get; set; } = TipoTarefa.Recorrente;
    public DateTimeOffset? ValidadeAvulsa { get; set; }
    public NaturezaTarefa Natureza { get; set; } = NaturezaTarefa.Bonus;
    public decimal? ValorMulta { get; set; }
    public bool RequerAprovacao { get; set; } = true;
    public TipoEvidencia TipoEvidencia { get; set; } = TipoEvidencia.Foto;
    public ProvedorIntegracao? ProvedorIntegracao { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Categoria? Categoria { get; set; }
    public ICollection<TarefaUsuario> Aderencias { get; set; } = new List<TarefaUsuario>();
}
