using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>
/// Tenant-scoped (AppDbContext / mesada_app / RLS) — não recebe familiaId
/// de propósito: a política de RLS em `familias` já restringe a uma única
/// linha (a própria), ver 019_rls_policies.sql.
/// </summary>
public interface IMinhaFamiliaRepository
{
    Task<Familia?> ObterAsync(CancellationToken ct = default);
}
