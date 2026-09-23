using Mesada.Domain.Enums;

namespace Mesada.Api.Contracts;

public sealed record DestinatarioNotificacaoResponse(Guid Id, TipoDestinatarioNotificacao Tipo, string Valor, bool Ativo);

public sealed record AdicionarDestinatarioNotificacaoRequest(TipoDestinatarioNotificacao Tipo, string Valor);
