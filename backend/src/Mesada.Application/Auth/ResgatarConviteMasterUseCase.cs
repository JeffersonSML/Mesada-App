using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Domain.Enums;

namespace Mesada.Application.Auth;

public sealed record ResultadoResgateConviteMaster(string Token, Guid FamiliaId, Guid UsuarioMasterId);

/// <summary>
/// Resgate de convite por um segundo responsável (papel Master):
/// diferente do resgate Comum (que só vincula um dispositivo a um
/// UsuarioComum já cadastrado), aqui a pessoa ainda não existe no sistema —
/// ela informa e-mail/senha para criar a própria conta. Nasce sem
/// IsFinanceiro (só o primeiro Master da família nasce financeiro, via
/// CriarFamiliaUseCase); tornar-se financeiro é uma ação separada.
/// </summary>
public sealed class ResgatarConviteMasterUseCase(
    IConviteAcessoRepository convites,
    IUsuarioMasterRepository masters,
    IPasswordHasher hasher,
    IJwtTokenService tokens,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    private const int TamanhoMinimoSenha = 8;

    public async Task<ResultadoResgateConviteMaster> ExecutarAsync(string codigo, string email, string senha, CancellationToken ct = default)
    {
        if (senha.Length < TamanhoMinimoSenha)
            throw new ValidacaoException($"Senha deve ter ao menos {TamanhoMinimoSenha} caracteres.");

        var convite = await convites.ObterPorCodigoAsync(codigo, ct)
            ?? throw new ConviteInvalidoException("Código de convite não encontrado.");

        if (convite.Status != StatusConvite.Pendente)
            throw new ConviteInvalidoException("Convite já utilizado, expirado ou revogado.");

        if (convite.ExpiraEm <= clock.UtcNow)
        {
            convite.Status = StatusConvite.Expirado;
            await unitOfWork.SaveChangesAsync(ct);
            throw new ConviteInvalidoException("Convite expirado — peça ao responsável para reemitir.");
        }

        if (convite.PapelAlvo != PapelConvite.Master)
            throw new ConviteInvalidoException("Este convite não é de acesso de responsável (Master).");

        if (await masters.ObterPorEmailAsync(email, ct) is not null)
            throw new EmailJaCadastradoException(email);

        var master = new UsuarioMaster
        {
            Id = Guid.NewGuid(),
            FamiliaId = convite.FamiliaId,
            Nome = convite.NomeConvidado ?? email,
            Email = email,
            SenhaHash = hasher.Hash(senha),
            IsFinanceiro = false,
        };
        masters.Adicionar(master);

        convite.Status = StatusConvite.Utilizado;
        convite.UtilizadoEm = clock.UtcNow;

        await unitOfWork.SaveChangesAsync(ct);

        var token = tokens.GerarTokenMaster(master);
        return new ResultadoResgateConviteMaster(token, master.FamiliaId, master.Id);
    }
}
