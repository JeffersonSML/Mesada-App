namespace Mesada.Application.Abstractions;

/// <summary>
/// Dá à Infraestrutura a família do usuário autenticado da requisição atual,
/// sem que Infrastructure precise referenciar ASP.NET Core/HttpContext
/// diretamente. Implementado em Mesada.Api (lê os claims do JWT) e
/// consumido pelo TenantConnectionInterceptor para popular
/// app.current_familia_id (docs/adr/0002-schema-e-rls.md).
/// </summary>
public interface ITenantContextAccessor
{
    Guid? FamiliaId { get; }

    /// <summary>Id do UsuarioMaster autenticado na requisição atual, quando o papel for Master — usado por casos de uso que registram "quem fez" (ex.: ConviteAcesso.CriadoPor), não pelo TenantConnectionInterceptor.</summary>
    Guid? UsuarioMasterId { get; }
}
