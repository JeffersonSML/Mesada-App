using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>
/// Lista os filhos (UsuarioComum) da família do usuário autenticado. Não
/// recebe familiaId como parâmetro de propósito: a implementação roda sobre
/// AppDbContext (role mesada_app), e é a política de Row Level Security —
/// não um filtro em C# — quem garante que só a própria família aparece
/// (docs/adr/0002-schema-e-rls.md).
/// </summary>
public interface IFilhosDaFamiliaQuery
{
    Task<IReadOnlyList<UsuarioComum>> ListarAtivosAsync(CancellationToken ct = default);
}
