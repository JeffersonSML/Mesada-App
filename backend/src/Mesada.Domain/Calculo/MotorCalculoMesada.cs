using Mesada.Domain.Enums;

namespace Mesada.Domain.Calculo;

/// <summary>
/// Implementa as fórmulas de docs/especificacao.md#lógica-de-cálculo. Puro
/// (sem I/O, sem dependência de EF Core ou de qualquer infraestrutura) —
/// toda a superfície de teste desta etapa passa por aqui.
/// </summary>
public static class MotorCalculoMesada
{
    private const int CasasDecimais = 2;

    /// <summary>
    /// Valor_tarefa = Valor_cadastrado × (%Conclusao / 100)                         [Valor Direto]
    /// Valor_tarefa = (Pontos_cadastrados × Valor_ponto) × (%Conclusao / 100)        [Pontos]
    /// </summary>
    public static decimal CalcularValorTarefa(
        ModoCalculo modo,
        decimal? valor,
        decimal? pontos,
        decimal? valorPonto,
        decimal percentualConclusao)
    {
        ValidarPercentual(percentualConclusao);

        return modo switch
        {
            ModoCalculo.ValorDireto => CalcularValorDireto(valor, percentualConclusao),
            ModoCalculo.Pontos => CalcularValorPontos(pontos, valorPonto, percentualConclusao),
            _ => throw new ArgumentOutOfRangeException(nameof(modo), modo, "Modo de cálculo desconhecido.")
        };
    }

    private static decimal CalcularValorDireto(decimal? valor, decimal percentual)
    {
        if (valor is null)
            throw new InvalidOperationException("Tarefa em modo Valor Direto precisa ter 'valor' cadastrado.");

        return Arredondar(valor.Value * (percentual / 100m));
    }

    private static decimal CalcularValorPontos(decimal? pontos, decimal? valorPonto, decimal percentual)
    {
        if (pontos is null)
            throw new InvalidOperationException("Tarefa em modo Pontos precisa ter 'pontos' cadastrados.");
        if (valorPonto is null)
            throw new InvalidOperationException("Valor do ponto não foi configurado para esta família/filho.");

        return Arredondar(pontos.Value * valorPonto.Value * (percentual / 100m));
    }

    /// <summary>
    /// Multa aplicável apenas a tarefas Obrigatórias marcadas como Não feito —
    /// configurável, independente do valor que a tarefa geraria se concluída.
    /// </summary>
    public static decimal CalcularMulta(NaturezaTarefa natureza, StatusExecucao status, decimal? valorMulta)
    {
        if (natureza != NaturezaTarefa.Obrigatoria || status != StatusExecucao.NaoFeito)
            return 0m;

        return valorMulta ?? 0m;
    }

    /// <summary>
    /// Valor_sugerido = Mesada_base / N_tarefas_aderentes   [Modo Valor Direto]
    /// </summary>
    public static decimal SugerirValorDireto(decimal mesadaBase, int quantidadeTarefasAderentes)
    {
        ValidarQuantidadeAderentes(quantidadeTarefasAderentes);
        return Arredondar(mesadaBase / quantidadeTarefasAderentes);
    }

    /// <summary>
    /// Pontos_totais = Mesada_base / Valor_ponto
    /// Pontos_sugeridos = Pontos_totais / N_tarefas_aderentes   [Modo Pontos]
    /// </summary>
    public static decimal SugerirPontos(decimal mesadaBase, decimal valorPonto, int quantidadeTarefasAderentes)
    {
        ValidarQuantidadeAderentes(quantidadeTarefasAderentes);
        if (valorPonto <= 0)
            throw new ArgumentOutOfRangeException(nameof(valorPonto), valorPonto, "Valor do ponto deve ser positivo.");

        var pontosTotais = mesadaBase / valorPonto;
        return Arredondar(pontosTotais / quantidadeTarefasAderentes);
    }

    /// <summary>
    /// Agrega os itens de um ciclo em soma de bônus e soma de multas.
    /// Regra (consistente com o modelo de negócio da spec): apenas tarefas de
    /// natureza Bônus, quando cumpridas (Feito/Parcial), somam valor extra à
    /// mesada; tarefas Obrigatórias cumpridas não somam nada além do
    /// esperado — só geram multa quando marcadas como Não feito.
    /// </summary>
    public static ResumoCiclo CalcularResumoCiclo(IEnumerable<ItemExecucaoCiclo> itens)
    {
        ArgumentNullException.ThrowIfNull(itens);

        decimal somaBonus = 0m;
        decimal somaMultas = 0m;

        foreach (var item in itens)
        {
            if (item.Status == StatusExecucao.NaoFeito)
            {
                somaMultas += CalcularMulta(item.Natureza, item.Status, item.ValorMulta);
                continue;
            }

            if (item.Status is not (StatusExecucao.Feito or StatusExecucao.Parcial))
                continue; // Pendente ainda não apurado — não entra no fechamento.

            if (item.Natureza != NaturezaTarefa.Bonus)
                continue; // Obrigatória cumprida: sem bônus extra, só evita a multa.

            somaBonus += CalcularValorTarefa(
                item.ModoCalculo, item.Valor, item.Pontos, item.ValorPonto, item.PercentualConclusao);
        }

        return new ResumoCiclo(Arredondar(somaBonus), Arredondar(somaMultas));
    }

    /// <summary>
    /// Mesada_final = Mesada_base + Σ Valor_bonus − Σ Valor_multa, deduzida
    /// do débito acumulado do ciclo anterior. Se o resultado for negativo, o
    /// valor pago zera e o débito remanescente é acumulado para o próximo
    /// ciclo (docs/especificacao.md#lógica-de-cálculo e #ciclo-de-mesada —
    /// ver também docs/adr/0002-schema-e-rls.md).
    /// </summary>
    public static ResultadoFechamentoCiclo FecharCiclo(
        decimal mesadaBase,
        decimal somaBonus,
        decimal somaMultas,
        decimal saldoDevedorAnterior)
    {
        if (saldoDevedorAnterior < 0)
            throw new ArgumentOutOfRangeException(nameof(saldoDevedorAnterior), saldoDevedorAnterior, "Saldo devedor não pode ser negativo.");

        var brutoCiclo = mesadaBase + somaBonus - somaMultas;
        var apósDebito = Arredondar(brutoCiclo - saldoDevedorAnterior);

        return apósDebito >= 0
            ? new ResultadoFechamentoCiclo(apósDebito, 0m)
            : new ResultadoFechamentoCiclo(0m, -apósDebito);
    }

    private static void ValidarPercentual(decimal percentual)
    {
        if (percentual is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percentual), percentual, "Percentual de conclusão deve estar entre 0 e 100.");
    }

    private static void ValidarQuantidadeAderentes(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade), quantidade, "Quantidade de tarefas aderentes deve ser maior que zero.");
    }

    private static decimal Arredondar(decimal valor) => Math.Round(valor, CasasDecimais, MidpointRounding.AwayFromZero);
}
