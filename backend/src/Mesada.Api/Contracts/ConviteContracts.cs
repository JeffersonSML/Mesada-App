using Mesada.Domain.Enums;

namespace Mesada.Api.Contracts;

public sealed record ConviteResponse(
    Guid Id,
    string Codigo,
    PapelConvite PapelAlvo,
    Guid? UsuarioComumId,
    StatusConvite Status,
    DateTimeOffset ExpiraEm);

public sealed record CriarConviteRequest(Guid UsuarioComumId);
