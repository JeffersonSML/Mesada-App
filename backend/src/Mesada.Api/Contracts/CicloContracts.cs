namespace Mesada.Api.Contracts;

public sealed record FecharCicloRequest(Guid UsuarioComumId, DateOnly DataInicio, DateOnly DataFim);

public sealed record CicloResponse(
    Guid Id,
    Guid UsuarioComumId,
    DateOnly DataInicio,
    DateOnly DataFim,
    decimal MesadaBase,
    decimal SomaBonus,
    decimal SomaMultas,
    decimal SaldoDevedorAnterior,
    decimal? ValorFinal,
    decimal SaldoDevedorResultante);

public sealed record CicloAtualResponse(
    DateOnly DataInicio,
    DateOnly DataFim,
    decimal MesadaBase,
    decimal SomaBonus,
    decimal SomaMultas,
    decimal SaldoDevedorAnterior,
    decimal ValorFinalPrevisto,
    decimal SaldoDevedorPrevisto);
