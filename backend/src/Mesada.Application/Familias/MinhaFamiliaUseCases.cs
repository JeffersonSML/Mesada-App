using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Domain.Enums;

namespace Mesada.Application.Familias;

public sealed class ObterMinhaFamiliaUseCase(IMinhaFamiliaRepository familia)
{
    public async Task<Familia> ExecutarAsync(CancellationToken ct = default) =>
        await familia.ObterAsync(ct) ?? throw new InvalidOperationException("Família do usuário autenticado não existe mais.");
}

public sealed class AtualizarMinhaFamiliaUseCase(
    IMinhaFamiliaRepository familiaRepositorio,
    ITenantUnitOfWork unitOfWork)
{
    public async Task<Familia> ExecutarAsync(string nome, CicloPeriodicidade cicloFechamentoPadrao, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("Nome da família é obrigatório.");

        var familia = await familiaRepositorio.ObterAsync(ct)
            ?? throw new InvalidOperationException("Família do usuário autenticado não existe mais.");

        familia.Nome = nome;
        familia.CicloFechamentoPadrao = cicloFechamentoPadrao;

        await unitOfWork.SaveChangesAsync(ct);
        return familia;
    }
}
