namespace Mesada.Api.Contracts;

public sealed record GrupoAdministradorResponse(
    Guid Id, string Nome, string? Descricao, IReadOnlyDictionary<string, bool> Permissoes, bool Sistema);

public sealed record SalvarGrupoAdministradorRequest(
    string Nome, string? Descricao, IReadOnlyDictionary<string, bool>? Permissoes);

public sealed record AdministradorResponse(
    Guid Id, string Nome, string Email, string Status, bool DeveTrocarSenha, Guid? GrupoId, string? GrupoNome);

public sealed record ConvidarAdministradorRequest(string Nome, string Email, Guid GrupoId);

public sealed record ConviteAdministradorResponse(AdministradorResponse Administrador, string SenhaTemporaria);

public sealed record AtualizarAdministradorRequest(string Nome, Guid GrupoId);
