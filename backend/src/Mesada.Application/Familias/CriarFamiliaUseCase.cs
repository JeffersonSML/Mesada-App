using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Entities;

namespace Mesada.Application.Familias;

public sealed record ResultadoCriacaoFamilia(string Token, Guid FamiliaId, Guid UsuarioMasterId);

/// <summary>
/// Signup: cria a Família e seu primeiro Master, sempre financeiro
/// (docs/especificacao.md#cadastro-e-onboarding) — cadastro exclusivo da
/// Web, roda antes de existir qualquer tenant, então usa os repositórios
/// admin-backed (AdminDbContext / mesada_admin), como login e resgate de
/// convite (docs/adr/0003-autenticacao-e-tenant-context.md).
/// </summary>
public sealed class CriarFamiliaUseCase(
    IFamiliaRepository familias,
    IUsuarioMasterRepository masters,
    IPasswordHasher hasher,
    IJwtTokenService tokens,
    IUnitOfWork unitOfWork)
{
    private const int TamanhoMinimoSenha = 8;

    public async Task<ResultadoCriacaoFamilia> ExecutarAsync(
        string nomeFamilia, string nomeMaster, string emailMaster, string senhaMaster, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nomeFamilia))
            throw new ValidacaoException("Nome da família é obrigatório.");
        if (string.IsNullOrWhiteSpace(nomeMaster))
            throw new ValidacaoException("Nome do responsável é obrigatório.");
        if (senhaMaster.Length < TamanhoMinimoSenha)
            throw new ValidacaoException($"Senha deve ter ao menos {TamanhoMinimoSenha} caracteres.");

        if (await masters.ObterPorEmailAsync(emailMaster, ct) is not null)
            throw new EmailJaCadastradoException(emailMaster);

        var familia = new Familia { Id = Guid.NewGuid(), Nome = nomeFamilia };
        familias.Adicionar(familia);

        var master = new UsuarioMaster
        {
            Id = Guid.NewGuid(),
            FamiliaId = familia.Id,
            Nome = nomeMaster,
            Email = emailMaster,
            SenhaHash = hasher.Hash(senhaMaster),
            IsFinanceiro = true,
        };
        masters.Adicionar(master);

        await unitOfWork.SaveChangesAsync(ct);

        var token = tokens.GerarTokenMaster(master);
        return new ResultadoCriacaoFamilia(token, familia.Id, master.Id);
    }
}
