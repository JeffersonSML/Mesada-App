using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>
/// Tenant-scoped (AppDbContext / mesada_app / RLS) — distinto de
/// IConviteAcessoRepository (admin-backed, usado só no resgate por código,
/// antes de existir contexto de família). Este é usado pelo Master para
/// gerenciar os convites da própria família: criar, listar, revogar.
/// </summary>
public interface IConvitesRepository
{
    Task<IReadOnlyList<ConviteAcesso>> ListarAsync(CancellationToken ct = default);

    Task<ConviteAcesso?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    void Adicionar(ConviteAcesso convite);
}
