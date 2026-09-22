using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

public interface IConviteAcessoRepository
{
    Task<ConviteAcesso?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default);

    void Adicionar(ConviteAcesso convite);
}
