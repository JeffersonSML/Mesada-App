using Mesada.Domain.Enums;

namespace Mesada.Domain.Entities;

public class ConviteAcesso
{
    public Guid Id { get; set; }
    public Guid FamiliaId { get; set; }
    public string Codigo { get; set; } = default!;
    public PapelConvite PapelAlvo { get; set; }
    public Guid? UsuarioComumId { get; set; }
    public string? NomeConvidado { get; set; }
    public StatusConvite Status { get; set; } = StatusConvite.Pendente;
    public Guid CriadoPor { get; set; }
    public DateTimeOffset? UtilizadoEm { get; set; }
    public string? DispositivoVinculado { get; set; }
    public DateTimeOffset ExpiraEm { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public UsuarioComum? UsuarioComum { get; set; }
}
