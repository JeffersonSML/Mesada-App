using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

public interface IUsuarioComumRepository
{
    Task<UsuarioComum?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    void Adicionar(UsuarioComum usuarioComum);
}
