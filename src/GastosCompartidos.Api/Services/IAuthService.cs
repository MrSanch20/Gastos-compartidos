using GastosCompartidos.Api.DTOs.Auth;

namespace GastosCompartidos.Api.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegistrarAsync(RegistroRequest request, CancellationToken ct);
    Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken ct);
}
