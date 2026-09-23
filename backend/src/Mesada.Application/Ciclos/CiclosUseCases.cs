using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Calculo;
using Mesada.Domain.Entities;
using Mesada.Domain.Enums;

namespace Mesada.Application.Ciclos;

public sealed record PreviaCicloAtual(
    DateOnly DataInicio, DateOnly DataFim, decimal MesadaBase, decimal SomaBonus, decimal SomaMultas,
    decimal SaldoDevedorAnterior, decimal ValorFinalPrevisto, decimal SaldoDevedorPrevisto);

/// <summary>Compartilhado por FecharCicloUseCase e ObterCicloAtualUseCase: converte Execucao + navegações em ItemExecucaoCiclo, o formato que MotorCalculoMesada entende.</summary>
internal static class ItensCicloBuilder
{
    public static IEnumerable<ItemExecucaoCiclo> Construir(IEnumerable<Execucao> execucoes, decimal? valorPontoDoFilho) =>
        execucoes.Select(e =>
        {
            var tarefa = e.TarefaUsuario!.Tarefa!;
            var valorEfetivo = e.TarefaUsuario.ValorOverride ?? tarefa.Valor;
            var pontosEfetivo = e.TarefaUsuario.PontosOverride ?? tarefa.Pontos;
            return new ItemExecucaoCiclo(
                tarefa.Natureza, tarefa.ModoCalculo, valorEfetivo, pontosEfetivo, valorPontoDoFilho,
                tarefa.ValorMulta, e.Status, e.PercentualConclusao);
        });
}

/// <summary>
/// Fechamento de ciclo (docs/especificacao.md#ciclo-de-mesada e
/// #lógica-de-cálculo): consolida as execuções ainda não vinculadas a
/// nenhum ciclo dentro do período informado, aplica MotorCalculoMesada e
/// atualiza o saldo devedor acumulado do filho para o próximo ciclo. As
/// datas do período são decididas pelo Master explicitamente — calcular
/// automaticamente a partir de CicloPeriodicidade fica para uma etapa
/// futura, quando o produto tiver um agendador de fechamento.
/// </summary>
public sealed class FecharCicloUseCase(
    ICiclosRepository ciclos,
    IExecucoesRepository execucoes,
    IFilhosRepository filhos,
    ITenantContextAccessor tenantContext,
    ITenantUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<CicloMesada> ExecutarAsync(Guid usuarioComumId, DateOnly dataInicio, DateOnly dataFim, CancellationToken ct = default)
    {
        if (dataFim < dataInicio)
            throw new ValidacaoException("Data de fim não pode ser anterior à data de início.");

        var filho = await filhos.ObterPorIdAsync(usuarioComumId, ct)
            ?? throw new RecursoNaoEncontradoException("Filho não encontrado.");

        if (await ciclos.ExisteComDataInicioAsync(usuarioComumId, dataInicio, ct))
            throw new ValidacaoException("Já existe um ciclo com esta data de início para este filho.");

        var execucoesDoPeriodo = await execucoes.ListarNaoConsolidadasAsync(usuarioComumId, dataInicio, dataFim, ct);

        var itens = ItensCicloBuilder.Construir(execucoesDoPeriodo, filho.ValorPonto);
        var resumo = MotorCalculoMesada.CalcularResumoCiclo(itens);
        var resultado = MotorCalculoMesada.FecharCiclo(filho.MesadaBase, resumo.SomaBonus, resumo.SomaMultas, filho.SaldoDevedorAcumulado);

        var ciclo = new CicloMesada
        {
            Id = Guid.NewGuid(),
            FamiliaId = tenantContext.FamiliaId ?? throw new InvalidOperationException("Requisição sem contexto de família."),
            UsuarioComumId = usuarioComumId,
            DataInicio = dataInicio,
            DataFim = dataFim,
            MesadaBase = filho.MesadaBase,
            SomaBonus = resumo.SomaBonus,
            SomaMultas = resumo.SomaMultas,
            SaldoDevedorAnterior = filho.SaldoDevedorAcumulado,
            ValorFinal = resultado.ValorFinal,
            SaldoDevedorResultante = resultado.SaldoDevedorResultante,
            Status = StatusCiclo.Fechado,
            FechadoEm = clock.UtcNow,
        };
        ciclos.Adicionar(ciclo);

        foreach (var execucao in execucoesDoPeriodo)
            execucao.CicloMesadaId = ciclo.Id;

        filho.SaldoDevedorAcumulado = resultado.SaldoDevedorResultante;

        await unitOfWork.SaveChangesAsync(ct);
        return ciclo;
    }
}

public sealed class ListarHistoricoCiclosUseCase(ICiclosRepository ciclos, IFilhosRepository filhos)
{
    public async Task<IReadOnlyList<CicloMesada>> ExecutarAsync(Guid usuarioComumId, CancellationToken ct = default)
    {
        if (await filhos.ObterPorIdAsync(usuarioComumId, ct) is null)
            throw new RecursoNaoEncontradoException("Filho não encontrado.");

        return await ciclos.ListarHistoricoAsync(usuarioComumId, ct);
    }
}

/// <summary>Prévia do ciclo em andamento — nada é persistido; mostra ao Master/filho quanto já foi acumulado desde o último fechamento.</summary>
public sealed class ObterCicloAtualUseCase(
    ICiclosRepository ciclos,
    IExecucoesRepository execucoes,
    IFilhosRepository filhos,
    IClock clock)
{
    public async Task<PreviaCicloAtual> ExecutarAsync(Guid usuarioComumId, CancellationToken ct = default)
    {
        var filho = await filhos.ObterPorIdAsync(usuarioComumId, ct)
            ?? throw new RecursoNaoEncontradoException("Filho não encontrado.");

        var ultimoFechado = await ciclos.ObterUltimoFechadoAsync(usuarioComumId, ct);
        var dataInicio = ultimoFechado is not null
            ? ultimoFechado.DataFim.AddDays(1)
            : DateOnly.FromDateTime(filho.CreatedAt.UtcDateTime);
        var dataFim = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        if (dataFim < dataInicio)
            dataFim = dataInicio;

        var execucoesDoPeriodo = await execucoes.ListarNaoConsolidadasAsync(usuarioComumId, dataInicio, dataFim, ct);

        var itens = ItensCicloBuilder.Construir(execucoesDoPeriodo, filho.ValorPonto);
        var resumo = MotorCalculoMesada.CalcularResumoCiclo(itens);
        var resultado = MotorCalculoMesada.FecharCiclo(filho.MesadaBase, resumo.SomaBonus, resumo.SomaMultas, filho.SaldoDevedorAcumulado);

        return new PreviaCicloAtual(
            dataInicio, dataFim, filho.MesadaBase, resumo.SomaBonus, resumo.SomaMultas,
            filho.SaldoDevedorAcumulado, resultado.ValorFinal, resultado.SaldoDevedorResultante);
    }
}
