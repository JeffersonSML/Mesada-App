namespace Mesada.Api.Contracts;

public sealed record FilhoResponse(Guid Id, string Nome, string? Apelido, decimal SaldoDevedorAcumulado);
