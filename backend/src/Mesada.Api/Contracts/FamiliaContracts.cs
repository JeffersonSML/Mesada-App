using Mesada.Domain.Enums;

namespace Mesada.Api.Contracts;

public sealed record CriarFamiliaRequest(string NomeFamilia, string NomeMaster, string EmailMaster, string SenhaMaster);

public sealed record FamiliaResponse(Guid Id, string Nome, CicloPeriodicidade CicloFechamentoPadrao);

public sealed record AtualizarFamiliaRequest(string Nome, CicloPeriodicidade CicloFechamentoPadrao);
