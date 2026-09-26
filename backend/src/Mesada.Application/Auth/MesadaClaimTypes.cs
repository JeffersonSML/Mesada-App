namespace Mesada.Application.Auth;

/// <summary>
/// Nomes de claims usados nos JWTs emitidos pelo sistema. familia_id é o
/// claim que a Api lê para popular app.current_familia_id a cada
/// requisição (ver docs/adr/0002-schema-e-rls.md) — nunca renomear sem
/// atualizar TenantContextMiddleware em Mesada.Api.
/// </summary>
public static class MesadaClaimTypes
{
    public const string FamiliaId = "familia_id";
    public const string Papel = "papel"; // "master" | "comum" | "admin"
    public const string UsuarioMasterId = "usuario_master_id";
    public const string UsuarioComumId = "usuario_comum_id";
    public const string AdministradorId = "administrador_id";
    public const string GrupoAdministradorId = "grupo_administrador_id";
    /// <summary>"true" quando o grupo do Administrador é o grupo sistema (Owner) — acesso total, sem checar permissoes jsonb.</summary>
    public const string GrupoAdministradorSistema = "grupo_administrador_sistema";
    public const string IsFinanceiro = "is_financeiro";
    public const string DispositivoId = "dispositivo_id";
}

public static class MesadaPapeis
{
    public const string Master = "master";
    public const string Comum = "comum";
    public const string Administrador = "admin";
}
