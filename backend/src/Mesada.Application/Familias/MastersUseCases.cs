using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Entities;

namespace Mesada.Application.Familias;

public sealed class ListarMastersUseCase(IMastersRepository masters)
{
    public Task<IReadOnlyList<UsuarioMaster>> ExecutarAsync(CancellationToken ct = default) =>
        masters.ListarAsync(ct);
}

/// <summary>
/// Desativação lógica de um segundo responsável (nunca do próprio autenticado,
/// nem do último Master financeiro ativo — sem isso, ninguém poderia gerir
/// assinatura/cobrança da família).
/// </summary>
public sealed class DesativarMasterUseCase(
    IMastersRepository masters,
    ITenantUnitOfWork unitOfWork,
    ITenantContextAccessor tenantContext)
{
    public async Task ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        if (id == tenantContext.UsuarioMasterId)
            throw new ValidacaoException("Você não pode desativar a própria conta.");

        var master = await masters.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Master não encontrado.");

        if (master.IsFinanceiro)
        {
            var todos = await masters.ListarAsync(ct);
            var financeirosAtivos = todos.Count(m => m.IsFinanceiro && m.Status == "ativo");
            if (financeirosAtivos <= 1)
                throw new ValidacaoException("Não é possível desativar o único Master financeiro ativo da família.");
        }

        master.Status = "inativo";
        await unitOfWork.SaveChangesAsync(ct);
    }
}
