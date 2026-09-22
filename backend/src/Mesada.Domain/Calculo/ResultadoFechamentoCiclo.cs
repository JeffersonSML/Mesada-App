namespace Mesada.Domain.Calculo;

public readonly record struct ResultadoFechamentoCiclo(decimal ValorFinal, decimal SaldoDevedorResultante);

public readonly record struct ResumoCiclo(decimal SomaBonus, decimal SomaMultas);

/// <summary>
/// Um item de execução já resolvido (tarefa + aderência + execução), no
/// formato mínimo que o motor de cálculo precisa para apurar um ciclo.
/// Construído pela Application a partir de entidades do EF Core — o motor
/// em si não depende de infraestrutura nenhuma.
/// </summary>
public sealed record ItemExecucaoCiclo(
    Enums.NaturezaTarefa Natureza,
    Enums.ModoCalculo ModoCalculo,
    decimal? Valor,
    decimal? Pontos,
    decimal? ValorPonto,
    decimal? ValorMulta,
    Enums.StatusExecucao Status,
    decimal PercentualConclusao);
