using Mesada.Domain.Enums;

namespace Mesada.Api.Contracts;

public sealed record FilhoResponse(Guid Id, string Nome, string? Apelido, decimal SaldoDevedorAcumulado);

public sealed record FilhoDetalheResponse(
    Guid Id,
    string Nome,
    string? Apelido,
    CicloPeriodicidade CicloFechamento,
    decimal MesadaBase,
    decimal? ValorPonto,
    decimal SaldoDevedorAcumulado);

public sealed record CriarFilhoRequest(
    string Nome,
    string? Apelido,
    CicloPeriodicidade CicloFechamento,
    decimal MesadaBase,
    decimal? ValorPonto);

public sealed record AtualizarFilhoRequest(
    string Nome,
    string? Apelido,
    CicloPeriodicidade CicloFechamento,
    decimal MesadaBase,
    decimal? ValorPonto);
