namespace Mesada.Application.Abstractions;

/// <summary>Abstrai a hora atual para tornar casos de uso com expiração/prazo testáveis.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
