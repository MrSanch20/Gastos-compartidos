using FluentValidation;
using GastosCompartidos.Api.DTOs.Auth;
using GastosCompartidos.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GastosCompartidos.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    IValidator<RegistroRequest> registroValidator,
    IValidator<LoginRequest> loginValidator) : ControllerBase
{
    [HttpPost("registro")]
    public async Task<ActionResult<AuthResponseDto>> Registro(RegistroRequest request, CancellationToken ct)
    {
        await registroValidator.ValidateAndThrowAsync(request, ct);
        var respuesta = await authService.RegistrarAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, respuesta);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequest request, CancellationToken ct)
    {
        await loginValidator.ValidateAndThrowAsync(request, ct);
        var respuesta = await authService.LoginAsync(request, ct);
        return Ok(respuesta);
    }
}
