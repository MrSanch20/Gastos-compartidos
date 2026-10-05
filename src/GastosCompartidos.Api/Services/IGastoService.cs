using GastosCompartidos.Api.DTOs;
using GastosCompartidos.Api.DTOs.Gastos;

namespace GastosCompartidos.Api.Services;

public interface IGastoService
{
    Task<PaginaResultado<GastoDto>> ListarAsync(
        int grupoId, int usuarioId, int pagina, int tamanoPagina,
        DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct);

    Task<GastoDto> CrearAsync(int grupoId, CrearGastoRequest request, int usuarioId, CancellationToken ct);
    Task<GastoDto> EditarAsync(int grupoId, int gastoId, ActualizarGastoRequest request, int usuarioId, CancellationToken ct);
    Task BorrarAsync(int grupoId, int gastoId, int usuarioId, CancellationToken ct);
}
