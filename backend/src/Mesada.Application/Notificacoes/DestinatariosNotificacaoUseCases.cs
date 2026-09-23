using System.Text.RegularExpressions;
using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Domain.Enums;

namespace Mesada.Application.Notificacoes;

public sealed class ListarDestinatariosNotificacaoUseCase(IDestinatariosNotificacaoRepository destinatarios)
{
    public Task<IReadOnlyList<NotificacaoDestinatario>> ExecutarAsync(CancellationToken ct = default) =>
        destinatarios.ListarAsync(ct);
}

/// <summary>
/// Cadastra um e-mail ou telefone adicional que passa a receber as
/// notificações da família (docs/especificacao.md#notificações) — tela do
/// Master, distinta da configuração de credenciais dos provedores (Resend/
/// FCM), que é segredo de infraestrutura e nunca fica exposta em endpoint.
/// </summary>
public sealed class AdicionarDestinatarioNotificacaoUseCase(
    IDestinatariosNotificacaoRepository destinatarios,
    ITenantContextAccessor tenantContext,
    ITenantUnitOfWork unitOfWork,
    IClock clock)
{
    private static readonly Regex RegexEmail = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex RegexTelefoneE164 = new(@"^\+[1-9]\d{7,14}$", RegexOptions.Compiled);

    public async Task<NotificacaoDestinatario> ExecutarAsync(TipoDestinatarioNotificacao tipo, string valor, CancellationToken ct = default)
    {
        valor = valor.Trim();
        ValidarFormato(tipo, valor);

        var destinatario = new NotificacaoDestinatario
        {
            Id = Guid.NewGuid(),
            FamiliaId = tenantContext.FamiliaId ?? throw new InvalidOperationException("Requisição sem contexto de família."),
            Tipo = tipo,
            Valor = valor,
            Ativo = true,
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
        };

        destinatarios.Adicionar(destinatario);
        await unitOfWork.SaveChangesAsync(ct);
        return destinatario;
    }

    private static void ValidarFormato(TipoDestinatarioNotificacao tipo, string valor)
    {
        var valido = tipo switch
        {
            TipoDestinatarioNotificacao.Email => RegexEmail.IsMatch(valor),
            TipoDestinatarioNotificacao.Telefone => RegexTelefoneE164.IsMatch(valor),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de destinatário desconhecido.")
        };

        if (!valido)
        {
            var esperado = tipo == TipoDestinatarioNotificacao.Email ? "e-mail válido" : "telefone em formato E.164 (ex.: +5511999999999)";
            throw new ValidacaoException($"Valor informado não é um {esperado}.");
        }
    }
}

public sealed class RemoverDestinatarioNotificacaoUseCase(
    IDestinatariosNotificacaoRepository destinatarios,
    ITenantUnitOfWork unitOfWork)
{
    public async Task ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        var destinatario = await destinatarios.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Destinatário de notificação não encontrado.");

        destinatarios.Remover(destinatario);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
