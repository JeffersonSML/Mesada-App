using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Domain.Enums;

namespace Mesada.Application.Familias;

/// <summary>Cadastro de um filho (UsuarioComum) pelo Master — cadastro exclusivo da Web.</summary>
public sealed class CriarFilhoUseCase(
    IFilhosRepository filhos,
    ITenantContextAccessor tenantContext,
    ITenantUnitOfWork unitOfWork)
{
    public async Task<UsuarioComum> ExecutarAsync(
        string nome, string? apelido, CicloPeriodicidade cicloFechamento, decimal mesadaBase, decimal? valorPonto,
        CancellationToken ct = default)
    {
        ValidarDadosBasicos(nome, mesadaBase, valorPonto);

        var filho = new UsuarioComum
        {
            Id = Guid.NewGuid(),
            FamiliaId = tenantContext.FamiliaId ?? throw new InvalidOperationException("Requisição sem contexto de família."),
            Nome = nome,
            Apelido = apelido,
            CicloFechamento = cicloFechamento,
            MesadaBase = mesadaBase,
            ValorPonto = valorPonto,
        };

        filhos.Adicionar(filho);
        await unitOfWork.SaveChangesAsync(ct);
        return filho;
    }

    internal static void ValidarDadosBasicos(string nome, decimal mesadaBase, decimal? valorPonto)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("Nome do filho é obrigatório.");
        if (mesadaBase < 0)
            throw new ValidacaoException("Mesada base não pode ser negativa.");
        if (valorPonto is <= 0)
            throw new ValidacaoException("Valor do ponto deve ser positivo.");
    }
}

public sealed class AtualizarFilhoUseCase(
    IFilhosRepository filhos,
    ITenantUnitOfWork unitOfWork)
{
    public async Task<UsuarioComum> ExecutarAsync(
        Guid id, string nome, string? apelido, CicloPeriodicidade cicloFechamento, decimal mesadaBase, decimal? valorPonto,
        CancellationToken ct = default)
    {
        CriarFilhoUseCase.ValidarDadosBasicos(nome, mesadaBase, valorPonto);

        var filho = await filhos.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Filho não encontrado.");

        filho.Nome = nome;
        filho.Apelido = apelido;
        filho.CicloFechamento = cicloFechamento;
        filho.MesadaBase = mesadaBase;
        filho.ValorPonto = valorPonto;

        await unitOfWork.SaveChangesAsync(ct);
        return filho;
    }
}
