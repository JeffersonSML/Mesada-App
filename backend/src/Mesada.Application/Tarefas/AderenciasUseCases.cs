using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Entities;

namespace Mesada.Application.Tarefas;

/// <summary>
/// Vincula um filho a uma tarefa (docs/especificacao.md
/// #cadastro-e-parametrização-de-tarefas) — só gera valor para quem está
/// aderente. Reativa uma aderência previamente removida em vez de duplicar
/// a linha, respeitando a UNIQUE (tarefa_id, usuario_comum_id).
/// </summary>
public sealed class AdicionarAderenciaUseCase(
    IAderenciasRepository aderencias,
    ITarefasRepository tarefas,
    IFilhosRepository filhos,
    ITenantContextAccessor tenantContext,
    ITenantUnitOfWork unitOfWork)
{
    public async Task<TarefaUsuario> ExecutarAsync(
        Guid tarefaId, Guid usuarioComumId, decimal? valorOverride, decimal? pontosOverride, CancellationToken ct = default)
    {
        if (valorOverride is < 0 || pontosOverride is < 0)
            throw new ValidacaoException("Overrides de valor/pontos não podem ser negativos.");

        if (await tarefas.ObterPorIdAsync(tarefaId, ct) is null)
            throw new RecursoNaoEncontradoException("Tarefa não encontrada.");
        if (await filhos.ObterPorIdAsync(usuarioComumId, ct) is null)
            throw new RecursoNaoEncontradoException("Filho não encontrado.");

        var existente = await aderencias.ObterAsync(tarefaId, usuarioComumId, ct);
        if (existente is not null)
        {
            if (existente.Ativo)
                throw new ValidacaoException("Este filho já está aderente a esta tarefa.");

            existente.Ativo = true;
            existente.ValorOverride = valorOverride;
            existente.PontosOverride = pontosOverride;
            await unitOfWork.SaveChangesAsync(ct);
            return existente;
        }

        var aderencia = new TarefaUsuario
        {
            Id = Guid.NewGuid(),
            FamiliaId = tenantContext.FamiliaId ?? throw new InvalidOperationException("Requisição sem contexto de família."),
            TarefaId = tarefaId,
            UsuarioComumId = usuarioComumId,
            ValorOverride = valorOverride,
            PontosOverride = pontosOverride,
            Ativo = true,
        };

        aderencias.Adicionar(aderencia);
        await unitOfWork.SaveChangesAsync(ct);
        return aderencia;
    }
}

public sealed class RemoverAderenciaUseCase(
    IAderenciasRepository aderencias,
    ITenantUnitOfWork unitOfWork)
{
    public async Task ExecutarAsync(Guid tarefaId, Guid usuarioComumId, CancellationToken ct = default)
    {
        var aderencia = await aderencias.ObterAsync(tarefaId, usuarioComumId, ct)
            ?? throw new RecursoNaoEncontradoException("Aderência não encontrada.");

        aderencia.Ativo = false;
        await unitOfWork.SaveChangesAsync(ct);
    }
}
