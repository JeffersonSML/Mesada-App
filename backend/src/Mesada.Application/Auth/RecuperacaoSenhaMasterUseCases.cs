using System.Security.Cryptography;
using System.Text;
using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Entities;

namespace Mesada.Application.Auth;

/// <summary>
/// "Esqueci minha senha" do Master — lacuna encontrada numa auditoria
/// funcional (diferente do Administrador, o Master não tinha nenhuma forma
/// de recuperar a senha). Nunca revela se o e-mail existe: sempre "concluído
/// com sucesso" do ponto de vista de quem chama, envia o e-mail só se
/// encontrar uma conta correspondente.
/// </summary>
public sealed class EsqueciSenhaMasterUseCase(
    IUsuarioMasterRepository masters,
    IRedefinicaoSenhaRepository redefinicoes,
    IEmailSender emailSender,
    IClock clock,
    IUnitOfWork unitOfWork,
    IAppUrlProvider appUrls)
{
    private static readonly TimeSpan Validade = TimeSpan.FromMinutes(30);

    public async Task ExecutarAsync(string email, CancellationToken ct = default)
    {
        var master = await masters.ObterPorEmailAsync(email, ct);
        if (master is null || master.Status != "ativo")
            return;

        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(tokenBytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');

        redefinicoes.Adicionar(new RedefinicaoSenhaMaster
        {
            Id = Guid.NewGuid(),
            UsuarioMasterId = master.Id,
            TokenHash = HashToken(token),
            ExpiraEm = clock.UtcNow.Add(Validade),
        });
        await unitOfWork.SaveChangesAsync(ct);

        var link = $"{appUrls.UrlBaseWeb.TrimEnd('/')}/redefinir-senha?token={Uri.EscapeDataString(token)}";
        var corpoHtml = $"""
            <p>Olá, {System.Net.WebUtility.HtmlEncode(master.Nome)}.</p>
            <p>Recebemos um pedido para redefinir a senha da sua conta no Mesada App.</p>
            <p><a href="{link}">Clique aqui para escolher uma nova senha</a>. Este link expira em 30 minutos.</p>
            <p>Se você não pediu isso, pode ignorar este e-mail com segurança.</p>
            """;
        await emailSender.EnviarAsync(master.Email, "Redefinição de senha — Mesada App", corpoHtml, ct);
    }

    internal static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}

/// <summary>Segunda etapa do "esqueci minha senha": troca a senha usando o token recebido por e-mail.</summary>
public sealed class RedefinirSenhaMasterUseCase(
    IRedefinicaoSenhaRepository redefinicoes,
    IUsuarioMasterRepository masters,
    IPasswordHasher hasher,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    private const int TamanhoMinimoSenha = 8;

    public async Task ExecutarAsync(string token, string novaSenha, CancellationToken ct = default)
    {
        if (novaSenha.Length < TamanhoMinimoSenha)
            throw new ValidacaoException($"Nova senha deve ter ao menos {TamanhoMinimoSenha} caracteres.");

        var tokenHash = EsqueciSenhaMasterUseCase.HashToken(token);
        var redefinicao = await redefinicoes.ObterPorTokenHashAsync(tokenHash, ct);

        if (redefinicao is null || redefinicao.UtilizadoEm is not null || redefinicao.ExpiraEm <= clock.UtcNow)
            throw new ValidacaoException("Link de redefinição inválido ou expirado. Peça um novo.");

        var master = await masters.ObterPorIdAsync(redefinicao.UsuarioMasterId, ct)
            ?? throw new ValidacaoException("Link de redefinição inválido ou expirado. Peça um novo.");

        master.SenhaHash = hasher.Hash(novaSenha);
        redefinicao.UtilizadoEm = clock.UtcNow;
        await unitOfWork.SaveChangesAsync(ct);
    }
}

/// <summary>Troca a própria senha (Master já autenticado, informando a senha atual) — não depende de e-mail, disponível a qualquer momento.</summary>
public sealed class TrocarSenhaMasterUseCase(
    IMastersRepository masters,
    IPasswordHasher hasher,
    ITenantUnitOfWork unitOfWork,
    ITenantContextAccessor tenantContext)
{
    private const int TamanhoMinimoSenha = 8;

    public async Task ExecutarAsync(string senhaAtual, string novaSenha, CancellationToken ct = default)
    {
        if (novaSenha.Length < TamanhoMinimoSenha)
            throw new ValidacaoException($"Nova senha deve ter ao menos {TamanhoMinimoSenha} caracteres.");

        var usuarioMasterId = tenantContext.UsuarioMasterId
            ?? throw new InvalidOperationException("Requisição sem Master autenticado.");
        var master = await masters.ObterPorIdAsync(usuarioMasterId, ct)
            ?? throw new InvalidOperationException("Master autenticado não existe mais.");

        if (!hasher.Verificar(senhaAtual, master.SenhaHash))
            throw new AutenticacaoInvalidaException();

        master.SenhaHash = hasher.Hash(novaSenha);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
