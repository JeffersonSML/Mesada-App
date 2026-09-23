namespace Mesada.Api.Contracts;

public sealed record CategoriaResponse(Guid Id, Guid? ParentId, string Nome, bool Sistema);

public sealed record CriarCategoriaRequest(string Nome, Guid? ParentId);

public sealed record AtualizarCategoriaRequest(string Nome);
