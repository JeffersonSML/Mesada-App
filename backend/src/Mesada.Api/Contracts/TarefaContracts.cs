using Mesada.Domain.Enums;

namespace Mesada.Api.Contracts;

public sealed record TarefaResponse(
    Guid Id,
    Guid CategoriaId,
    string Nome,
    string? Descricao,
    ModoCalculo ModoCalculo,
    decimal? Valor,
    decimal? Pontos,
    bool PermiteParcial,
    TipoTarefa Tipo,
    DateTimeOffset? ValidadeAvulsa,
    NaturezaTarefa Natureza,
    decimal? ValorMulta,
    bool RequerAprovacao,
    TipoEvidencia TipoEvidencia,
    ProvedorIntegracao? ProvedorIntegracao);

public sealed record SalvarTarefaRequest(
    Guid CategoriaId,
    string Nome,
    string? Descricao,
    ModoCalculo ModoCalculo,
    decimal? Valor,
    decimal? Pontos,
    bool PermiteParcial,
    TipoTarefa Tipo,
    DateTimeOffset? ValidadeAvulsa,
    NaturezaTarefa Natureza,
    decimal? ValorMulta,
    bool RequerAprovacao,
    TipoEvidencia TipoEvidencia,
    ProvedorIntegracao? ProvedorIntegracao);

public sealed record AderenciaRequest(Guid UsuarioComumId, decimal? ValorOverride, decimal? PontosOverride);

public sealed record AderenciaResponse(Guid TarefaId, Guid UsuarioComumId, decimal? ValorOverride, decimal? PontosOverride);

public sealed record SugestaoValorResponse(decimal Valor, ModoCalculo ModoCalculo);
