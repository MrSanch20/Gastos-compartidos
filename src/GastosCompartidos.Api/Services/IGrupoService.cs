using GastosCompartidos.Api.DTOs.Grupos;

namespace GastosCompartidos.Api.Services;

public interface IGrupoService
{
    Task<List<GrupoResumenDto>> ListarMisGruposAsync(int usuarioId, CancellationToken ct);
    Task<GrupoDetalleDto> ObtenerDetalleAsync(int grupoId, int usuarioId, CancellationToken ct);
    Task<GrupoResumenDto> CrearAsync(CrearGrupoRequest request, int usuarioCreadorId, CancellationToken ct);
    Task AnadirMiembroAsync(int grupoId, AnadirMiembroRequest request, int usuarioSolicitanteId, CancellationToken ct);
    Task QuitarMiembroAsync(int grupoId, int usuarioAQuitarId, int usuarioSolicitanteId, CancellationToken ct);
}
