namespace Mesada.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string senhaPlano);

    bool Verificar(string senhaPlano, string hash);
}
