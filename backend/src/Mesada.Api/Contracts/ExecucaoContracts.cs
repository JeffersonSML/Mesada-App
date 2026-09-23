using Mesada.Domain.Enums;

namespace Mesada.Api.Contracts;

public sealed record MarcarExecucaoRequest(Guid TarefaUsuarioId, StatusExecucao Status, decimal PercentualConclusao);

public sealed record ExecucaoResponse(
    Guid Id,
    Guid TarefaUsuarioId,
    DateTimeOffset DataExecucao,
    StatusExecucao Status,
    decimal PercentualConclusao,
    StatusAprovacao StatusAprovacao,
    decimal? ValorCalculado,
    decimal? PontosCalculado);

public sealed record MinhaTarefaResponse(
    Guid TarefaUsuarioId,
    Guid TarefaId,
    string Nome,
    string? Descricao,
    ModoCalculo ModoCalculo,
    decimal? Valor,
    decimal? Pontos,
    bool PermiteParcial,
    bool RequerAprovacao);
