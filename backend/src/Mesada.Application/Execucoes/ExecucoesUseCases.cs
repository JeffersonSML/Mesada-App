using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Calculo;
using Mesada.Domain.Entities;
using Mesada.Domain.Enums;

namespace Mesada.Application.Execucoes;

/// <summary>
/// Marca a conclusão de uma tarefa aderente (docs/especificacao.md
/// #fluxo-de-aprovação-de-tarefas) — ação do próprio filho (Comum) no app
/// mobile. O valor/pontos é calculado imediatamente (via MotorCalculoMesada,
/// usando *_override quando presente), mesmo que a tarefa exija aprovação:
/// serve de prévia para o Master, mas só entra no fechamento do ciclo
/// depois de aprovado (FecharCicloUseCase ignora execuções Pendente/Rejeitado).
/// </summary>
public sealed class MarcarExecucaoUseCase(
    IAderenciasRepository aderencias,
    IExecucoesRepository execucoes,
    IFilhosRepository filhos,
    ITenantContextAccessor tenantContext,
    ITenantUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Execucao> ExecutarAsync(
        Guid tarefaUsuarioId, StatusExecucao status, decimal percentualConclusao, CancellationToken ct = default)
    {
        if (percentualConclusao is < 0 or > 100)
            throw new ValidacaoException("Percentual de conclusão deve estar entre 0 e 100.");
        if (status == StatusExecucao.Pendente)
            throw new ValidacaoException("Status 'pendente' é o estado inicial, não pode ser marcado explicitamente.");

        var usuarioComumId = tenantContext.UsuarioComumId
            ?? throw new InvalidOperationException("Requisição sem usuário Comum autenticado.");

        var aderencia = await aderencias.ObterPorIdComTarefaAsync(tarefaUsuarioId, ct)
            ?? throw new RecursoNaoEncontradoException("Aderência não encontrada.");

        if (aderencia.UsuarioComumId != usuarioComumId)
            throw new ValidacaoException("Esta aderência não pertence ao usuário autenticado.");
        if (!aderencia.Ativo)
            throw new ValidacaoException("Esta aderência está inativa.");

        var tarefa = aderencia.Tarefa!;

        if (status == StatusExecucao.Parcial && (percentualConclusao == 100 || !tarefa.PermiteParcial))
            throw new ValidacaoException("Esta tarefa não permite conclusão parcial.");
        if (status == StatusExecucao.Feito && percentualConclusao != 100)
            throw new ValidacaoException("Status 'feito' exige percentual de conclusão igual a 100.");

        var filho = await filhos.ObterPorIdAsync(usuarioComumId, ct)
            ?? throw new InvalidOperationException("Usuário Comum autenticado não existe mais.");

        var (valorCalculado, pontosCalculado) = CalcularValores(tarefa, aderencia, filho, status, percentualConclusao);

        var execucao = new Execucao
        {
            Id = Guid.NewGuid(),
            FamiliaId = aderencia.FamiliaId,
            TarefaUsuarioId = tarefaUsuarioId,
            DataExecucao = clock.UtcNow,
            Status = status,
            PercentualConclusao = percentualConclusao,
            StatusAprovacao = tarefa.RequerAprovacao ? StatusAprovacao.Pendente : StatusAprovacao.NaoAplicavel,
            ValorCalculado = valorCalculado,
            PontosCalculado = pontosCalculado,
        };

        execucoes.Adicionar(execucao);
        await unitOfWork.SaveChangesAsync(ct);
        return execucao;
    }

    private static (decimal? Valor, decimal? Pontos) CalcularValores(
        Tarefa tarefa, TarefaUsuario aderencia, UsuarioComum filho, StatusExecucao status, decimal percentual)
    {
        // Espelha a regra de agregação de MotorCalculoMesada.CalcularResumoCiclo:
        // só tarefas Bônus cumpridas geram valor extra aqui; Obrigatória
        // cumprida não soma nada (só evita multa, apurada no fechamento do
        // ciclo); Não feito também não gera valor nesta execução.
        if (status == StatusExecucao.NaoFeito || tarefa.Natureza != NaturezaTarefa.Bonus)
            return (0m, 0m);

        var valorEfetivo = aderencia.ValorOverride ?? tarefa.Valor;
        var pontosEfetivo = aderencia.PontosOverride ?? tarefa.Pontos;

        var valor = MotorCalculoMesada.CalcularValorTarefa(tarefa.ModoCalculo, valorEfetivo, pontosEfetivo, filho.ValorPonto, percentual);
        return tarefa.ModoCalculo == ModoCalculo.Pontos ? (0m, valor) : (valor, 0m);
    }
}

public sealed class AprovarExecucaoUseCase(
    IExecucoesRepository execucoes,
    ITenantContextAccessor tenantContext,
    ITenantUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Execucao> ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        var execucao = await execucoes.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Execução não encontrada.");

        if (execucao.StatusAprovacao != StatusAprovacao.Pendente)
            throw new ValidacaoException("Só é possível aprovar uma execução pendente.");

        execucao.StatusAprovacao = StatusAprovacao.Aprovado;
        execucao.AprovadoPor = tenantContext.UsuarioMasterId
            ?? throw new InvalidOperationException("Requisição sem usuário Master autenticado.");
        execucao.AprovadoEm = clock.UtcNow;

        await unitOfWork.SaveChangesAsync(ct);
        return execucao;
    }
}

public sealed class RejeitarExecucaoUseCase(
    IExecucoesRepository execucoes,
    ITenantContextAccessor tenantContext,
    ITenantUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Execucao> ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        var execucao = await execucoes.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Execução não encontrada.");

        if (execucao.StatusAprovacao != StatusAprovacao.Pendente)
            throw new ValidacaoException("Só é possível rejeitar uma execução pendente.");

        execucao.StatusAprovacao = StatusAprovacao.Rejeitado;
        execucao.AprovadoPor = tenantContext.UsuarioMasterId
            ?? throw new InvalidOperationException("Requisição sem usuário Master autenticado.");
        execucao.AprovadoEm = clock.UtcNow;

        await unitOfWork.SaveChangesAsync(ct);
        return execucao;
    }
}

public sealed class ListarExecucoesPendentesUseCase(IExecucoesRepository execucoes)
{
    public Task<IReadOnlyList<Execucao>> ExecutarAsync(CancellationToken ct = default) =>
        execucoes.ListarPendentesAprovacaoAsync(ct);
}

/// <summary>Tarefas às quais o filho autenticado está aderente — usado pelo app mobile do Comum.</summary>
public sealed class ListarMinhasTarefasUseCase(
    IAderenciasRepository aderencias,
    ITenantContextAccessor tenantContext)
{
    public Task<IReadOnlyList<TarefaUsuario>> ExecutarAsync(CancellationToken ct = default)
    {
        var usuarioComumId = tenantContext.UsuarioComumId
            ?? throw new InvalidOperationException("Requisição sem usuário Comum autenticado.");

        return aderencias.ListarAtivasComTarefaPorUsuarioComumAsync(usuarioComumId, ct);
    }
}
