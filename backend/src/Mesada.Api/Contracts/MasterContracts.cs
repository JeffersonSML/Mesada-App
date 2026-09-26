namespace Mesada.Api.Contracts;

public sealed record MasterResponse(Guid Id, string Nome, string Email, bool IsFinanceiro, string Status);
