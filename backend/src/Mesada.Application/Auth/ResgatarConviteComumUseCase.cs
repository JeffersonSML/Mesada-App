using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Enums;

namespace Mesada.Application.Auth;

public sealed record ResultadoResgateConvite(string Token, Guid FamiliaId, Guid UsuarioComumId);

/// <summary>
/// Resgate de convite pelo usuário Comum (filho): mesmo mecanismo descrito em
/// docs/especificacao.md#fluxo-de-convite, mas só para papel_alvo = comum —
/// convites de Master resultam em criação de conta própria (e-mail/senha),
/// fluxo separado que ainda não está implementado nesta etapa.
/// </summary>
public sealed class ResgatarConviteComumUseCase(
    IConviteAcessoRepository convites,
    IUsuarioComumRepository usuariosComuns,
    IJwtTokenService tokens,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task<ResultadoResgateConvite> ExecutarAsync(string codigo, string dispositivoId, CancellationToken ct = default)
    {
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

        if (convite.PapelAlvo != PapelConvite.Comum || convite.UsuarioComumId is null)
            throw new ConviteInvalidoException("Este convite não é de acesso de filho (Comum).");

        var usuarioComum = await usuariosComuns.ObterPorIdAsync(convite.UsuarioComumId.Value, ct)
            ?? throw new InvalidOperationException("Usuário Comum referenciado pelo convite não existe mais.");

        usuarioComum.DispositivoVinculado = dispositivoId;
        convite.Status = StatusConvite.Utilizado;
        convite.UtilizadoEm = clock.UtcNow;
        convite.DispositivoVinculado = dispositivoId;

        await unitOfWork.SaveChangesAsync(ct);

        var token = tokens.GerarTokenComum(usuarioComum, dispositivoId);
        return new ResultadoResgateConvite(token, usuarioComum.FamiliaId, usuarioComum.Id);
    }
}
