using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GastosCompartidos.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GastosCompartidos.Api.Services;

/// <summary>
/// Crea los tokens JWT que identifican a un usuario autenticado en las siguientes peticiones.
/// </summary>
public class JwtGenerador(IConfiguration configuracion) : IJwtGenerador
{
    public (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario)
    {
        var clave = configuracion["Jwt:Clave"]
            ?? throw new InvalidOperationException("Falta configurar 'Jwt:Clave' (usa user-secrets en desarrollo).");
        var emisor = configuracion["Jwt:Emisor"];
        var audiencia = configuracion["Jwt:Audiencia"];
        var minutosExpiracion = int.Parse(configuracion["Jwt:MinutosExpiracion"] ?? "120");

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nombre)
        };

        var expiraEn = DateTime.UtcNow.AddMinutes(minutosExpiracion);

        var token = new JwtSecurityToken(
            issuer: emisor,
            audience: audiencia,
            claims: claims,
            expires: expiraEn,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEn);
    }
}
