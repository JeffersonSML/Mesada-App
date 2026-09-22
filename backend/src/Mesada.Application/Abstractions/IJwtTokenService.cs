using Mesada.Domain.Entities;

namespace Mesada.Application.Abstractions;

public interface IJwtTokenService
{
    /// <summary>Emite um JWT para um UsuarioMaster autenticado por e-mail/senha.</summary>
    string GerarTokenMaster(UsuarioMaster usuarioMaster);

    /// <summary>Emite um JWT para um UsuarioComum que resgatou um convite e vinculou o dispositivo.</summary>
    string GerarTokenComum(UsuarioComum usuarioComum, string dispositivoVinculado);

    /// <summary>Emite um JWT para um Administrador do sistema (fora do escopo de qualquer Familia).</summary>
    string GerarTokenAdministrador(Administrador administrador);
}
