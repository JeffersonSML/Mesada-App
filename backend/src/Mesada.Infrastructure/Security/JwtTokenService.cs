using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Mesada.Application.Abstractions;
using Mesada.Application.Auth;
using Mesada.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Mesada.Infrastructure.Security;

public sealed class JwtTokenService(IOptions<JwtOptions> options, IClock clock) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public string GerarTokenMaster(UsuarioMaster usuarioMaster) => GerarToken(
        expiracaoMinutos: _options.ExpiracaoMinutosMaster,
        claims:
        [
            new Claim(JwtRegisteredClaimNames.Sub, usuarioMaster.Id.ToString()),
            new Claim(MesadaClaimTypes.FamiliaId, usuarioMaster.FamiliaId.ToString()),
            new Claim(MesadaClaimTypes.UsuarioMasterId, usuarioMaster.Id.ToString()),
            new Claim(MesadaClaimTypes.Papel, MesadaPapeis.Master),
            new Claim(MesadaClaimTypes.IsFinanceiro, usuarioMaster.IsFinanceiro.ToString()),
        ]);

    public string GerarTokenComum(UsuarioComum usuarioComum, string dispositivoVinculado) => GerarToken(
        expiracaoMinutos: _options.ExpiracaoMinutosComum,
        claims:
        [
            new Claim(JwtRegisteredClaimNames.Sub, usuarioComum.Id.ToString()),
            new Claim(MesadaClaimTypes.FamiliaId, usuarioComum.FamiliaId.ToString()),
            new Claim(MesadaClaimTypes.UsuarioComumId, usuarioComum.Id.ToString()),
            new Claim(MesadaClaimTypes.Papel, MesadaPapeis.Comum),
            new Claim(MesadaClaimTypes.DispositivoId, dispositivoVinculado),
        ]);

    public string GerarTokenAdministrador(Administrador administrador) => GerarToken(
        expiracaoMinutos: _options.ExpiracaoMinutosAdministrador,
        claims:
        [
            new Claim(JwtRegisteredClaimNames.Sub, administrador.Id.ToString()),
            new Claim(MesadaClaimTypes.AdministradorId, administrador.Id.ToString()),
            new Claim(MesadaClaimTypes.Papel, MesadaPapeis.Administrador),
        ]);

    private string GerarToken(int expiracaoMinutos, Claim[] claims)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Chave));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var agora = clock.UtcNow;

        var token = new JwtSecurityToken(
            issuer: _options.Emissor,
            audience: _options.Audiencia,
            claims: claims,
            notBefore: agora.UtcDateTime,
            expires: agora.AddMinutes(expiracaoMinutos).UtcDateTime,
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
