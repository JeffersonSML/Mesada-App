using Mesada.Application.Abstractions;
using Mesada.Application.Auth;

namespace Mesada.Api.Tenancy;

/// <summary>Lê familia_id do JWT do usuário autenticado na requisição atual — implementação de ITenantContextAccessor que o TenantConnectionInterceptor (Infrastructure) consome.</summary>
public sealed class HttpTenantContextAccessor(IHttpContextAccessor httpContextAccessor) : ITenantContextAccessor
{
    public Guid? FamiliaId
    {
        get
        {
            var valor = httpContextAccessor.HttpContext?.User.FindFirst(MesadaClaimTypes.FamiliaId)?.Value;
            return Guid.TryParse(valor, out var id) ? id : null;
        }
    }

    public Guid? UsuarioMasterId
    {
        get
        {
            var valor = httpContextAccessor.HttpContext?.User.FindFirst(MesadaClaimTypes.UsuarioMasterId)?.Value;
            return Guid.TryParse(valor, out var id) ? id : null;
        }
    }

    public Guid? UsuarioComumId
    {
        get
        {
            var valor = httpContextAccessor.HttpContext?.User.FindFirst(MesadaClaimTypes.UsuarioComumId)?.Value;
            return Guid.TryParse(valor, out var id) ? id : null;
        }
    }

    public Guid? AdministradorId
    {
        get
        {
            var valor = httpContextAccessor.HttpContext?.User.FindFirst(MesadaClaimTypes.AdministradorId)?.Value;
            return Guid.TryParse(valor, out var id) ? id : null;
        }
    }

    public bool AdministradorEhOwner =>
        bool.TryParse(httpContextAccessor.HttpContext?.User.FindFirst(MesadaClaimTypes.GrupoAdministradorSistema)?.Value, out var ehOwner)
        && ehOwner;
}
