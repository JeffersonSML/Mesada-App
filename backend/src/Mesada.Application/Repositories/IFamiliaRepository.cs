using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

public interface IFamiliaRepository
{
    Task<Familia?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    void Adicionar(Familia familia);
}
