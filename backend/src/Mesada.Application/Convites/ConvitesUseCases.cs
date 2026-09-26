using System.Security.Cryptography;
using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Domain.Enums;

namespace Mesada.Application.Convites;

public sealed class ListarConvitesUseCase(IConvitesRepository convites)
{
    public Task<IReadOnlyList<ConviteAcesso>> ExecutarAsync(CancellationToken ct = default) =>
        convites.ListarAsync(ct);
}

/// <summary>
/// Gera um convite de acesso (docs/especificacao.md#fluxo-de-convite) —
/// para um filho (papel Comum) resgatar no app mobile, vinculando a um
/// UsuarioComum já cadastrado, ou para um segundo responsável (papel
/// Master) criar a própria conta com e-mail/senha na Web
/// (ResgatarConviteMasterUseCase). Exatamente um entre
/// <paramref name="usuarioComumId"/> (Comum) e <paramref name="nomeConvidado"/>
/// (Master) deve ser informado, conforme <paramref name="papelAlvo"/>.
/// </summary>
public sealed class CriarConviteUseCase(
    IConvitesRepository convites,
    IFilhosRepository filhos,
    ITenantContextAccessor tenantContext,
    ITenantUnitOfWork unitOfWork,
    IClock clock)
{
    private static readonly TimeSpan ValidadeConvite = TimeSpan.FromDays(7);
    private const string AlfabetoCodigo = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // sem caracteres ambíguos (0/O, 1/I/L)
    private const int TamanhoCodigo = 8;

    public async Task<ConviteAcesso> ExecutarAsync(
        PapelConvite papelAlvo, Guid? usuarioComumId, string? nomeConvidado, CancellationToken ct = default)
    {
        if (papelAlvo == PapelConvite.Comum)
        {
            if (usuarioComumId is null)
                throw new ValidacaoException("Convite de filho (Comum) exige 'usuarioComumId'.");
            _ = await filhos.ObterPorIdAsync(usuarioComumId.Value, ct)
                ?? throw new RecursoNaoEncontradoException("Filho não encontrado.");
        }
        else if (string.IsNullOrWhiteSpace(nomeConvidado))
        {
            throw new ValidacaoException("Convite de responsável (Master) exige 'nomeConvidado'.");
        }

        var masterId = tenantContext.UsuarioMasterId
            ?? throw new InvalidOperationException("Requisição sem usuário Master autenticado.");

        var convite = new ConviteAcesso
        {
            Id = Guid.NewGuid(),
            FamiliaId = tenantContext.FamiliaId ?? throw new InvalidOperationException("Requisição sem contexto de família."),
            Codigo = GerarCodigo(),
            PapelAlvo = papelAlvo,
            UsuarioComumId = papelAlvo == PapelConvite.Comum ? usuarioComumId : null,
            NomeConvidado = papelAlvo == PapelConvite.Master ? nomeConvidado : null,
            Status = StatusConvite.Pendente,
            CriadoPor = masterId,
            ExpiraEm = clock.UtcNow.Add(ValidadeConvite),
        };

        convites.Adicionar(convite);
        await unitOfWork.SaveChangesAsync(ct);
        return convite;
    }

    private static string GerarCodigo() =>
        new(RandomNumberGenerator.GetItems<char>(AlfabetoCodigo, TamanhoCodigo));
}

public sealed class RevogarConviteUseCase(
    IConvitesRepository convites,
    ITenantUnitOfWork unitOfWork)
{
    public async Task ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        var convite = await convites.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Convite não encontrado.");

        if (convite.Status != StatusConvite.Pendente)
            throw new ValidacaoException("Só é possível revogar um convite pendente.");

        convite.Status = StatusConvite.Revogado;
        await unitOfWork.SaveChangesAsync(ct);
    }
}
