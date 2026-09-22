namespace Mesada.Api.Contracts;

public sealed record LoginMasterRequest(string Email, string Senha);

public sealed record TokenResponse(string Token);

public sealed record ResgatarConviteRequest(string DispositivoId);
