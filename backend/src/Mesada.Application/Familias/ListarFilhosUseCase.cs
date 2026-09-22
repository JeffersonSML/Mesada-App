using Mesada.Application.Repositories;
using Mesada.Domain.Entities;

namespace Mesada.Application.Familias;

public sealed class ListarFilhosUseCase(IFilhosDaFamiliaQuery query)
{
    public Task<IReadOnlyList<UsuarioComum>> ExecutarAsync(CancellationToken ct = default) =>
        query.ListarAtivosAsync(ct);
}
