using GastosCompartidos.Api.Data;
using GastosCompartidos.Api.DTOs.Auth;
using GastosCompartidos.Api.Excepciones;
using GastosCompartidos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosCompartidos.Api.Services;

public class AuthService(GastosCompartidosDbContext contexto, IJwtGenerador jwtGenerador) : IAuthService
{
    public async Task<AuthResponseDto> RegistrarAsync(RegistroRequest request, CancellationToken ct)
    {
        var emailYaRegistrado = await contexto.Usuarios
            .AnyAsync(u => u.Email == request.Email, ct);

        if (emailYaRegistrado)
        {
            throw new ConflictoException("Ya existe un usuario registrado con ese email.");
        }

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync(ct);

        return CrearRespuesta(usuario);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var usuario = await contexto.Usuarios
            .SingleOrDefaultAsync(u => u.Email == request.Email, ct);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
        {
            throw new CredencialesInvalidasException("El email o la contraseña no son correctos.");
        }

        return CrearRespuesta(usuario);
    }

    private AuthResponseDto CrearRespuesta(Usuario usuario)
    {
        var (token, expiraEn) = jwtGenerador.GenerarToken(usuario);
        return new AuthResponseDto(token, expiraEn, usuario.Id, usuario.Nombre, usuario.Email);
    }
}
