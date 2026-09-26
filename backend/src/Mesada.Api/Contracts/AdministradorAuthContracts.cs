namespace Mesada.Api.Contracts;

public sealed record LoginAdministradorRequest(string Email, string Senha);

public sealed record TokenAdministradorResponse(string Token, bool DeveTrocarSenha);

public sealed record TrocarSenhaAdministradorRequest(string SenhaAtual, string NovaSenha);

public sealed record AtualizarEmailAdministradorRequest(string NovoEmail);
