using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace GastosCompartidos.Api.Controllers;

/// <summary>
/// Controller base con acceso cómodo al id del usuario autenticado a partir del token JWT.
/// </summary>
public abstract class ControladorBase : ControllerBase
{
    protected int UsuarioActualId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("El token no contiene el identificador del usuario."));
}
