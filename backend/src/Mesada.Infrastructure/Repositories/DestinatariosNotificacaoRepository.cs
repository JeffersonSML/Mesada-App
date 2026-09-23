using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>Backed por AppDbContext (mesada_app) — RLS restringe as linhas à própria família, sem filtro em C#.</summary>
public sealed class DestinatariosNotificacaoRepository(AppDbContext db) : IDestinatariosNotificacaoRepository
{
    public async Task<IReadOnlyList<NotificacaoDestinatario>> ListarAsync(CancellationToken ct = default) =>
        await db.NotificacaoDestinatarios
            .OrderBy(d => d.Tipo).ThenBy(d => d.Valor)
            .ToListAsync(ct);

    public Task<NotificacaoDestinatario?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.NotificacaoDestinatarios.SingleOrDefaultAsync(d => d.Id == id, ct);

    public void Adicionar(NotificacaoDestinatario destinatario) => db.NotificacaoDestinatarios.Add(destinatario);

    public void Remover(NotificacaoDestinatario destinatario) => db.NotificacaoDestinatarios.Remove(destinatario);
}
