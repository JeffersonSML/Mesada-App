using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>
/// Tenant-scoped (AppDbContext / mesada_app / RLS) — não recebe familiaId
/// como parâmetro de propósito, ver IFilhosDaFamiliaQuery.
/// </summary>
public interface IDestinatariosNotificacaoRepository
{
    Task<IReadOnlyList<NotificacaoDestinatario>> ListarAsync(CancellationToken ct = default);

    Task<NotificacaoDestinatario?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    void Adicionar(NotificacaoDestinatario destinatario);

    void Remover(NotificacaoDestinatario destinatario);
}
