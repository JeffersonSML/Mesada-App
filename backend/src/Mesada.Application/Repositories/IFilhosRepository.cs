using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>
/// Tenant-scoped (AppDbContext / mesada_app / RLS) — usado pelos casos de
/// uso de escrita (criar/editar filho); a leitura em lista continua em
/// IFilhosDaFamiliaQuery.
/// </summary>
public interface IFilhosRepository
{
    Task<UsuarioComum?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    void Adicionar(UsuarioComum usuarioComum);
}
