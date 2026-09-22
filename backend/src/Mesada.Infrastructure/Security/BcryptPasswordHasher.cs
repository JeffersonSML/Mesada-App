using Mesada.Application.Abstractions;

namespace Mesada.Infrastructure.Security;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string senhaPlano) => BCrypt.Net.BCrypt.HashPassword(senhaPlano, workFactor: 12);

    public bool Verificar(string senhaPlano, string hash) => BCrypt.Net.BCrypt.Verify(senhaPlano, hash);
}
