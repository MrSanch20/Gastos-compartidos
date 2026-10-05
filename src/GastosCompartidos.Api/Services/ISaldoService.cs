using GastosCompartidos.Api.DTOs.Saldos;

namespace GastosCompartidos.Api.Services;

public interface ISaldoService
{
    Task<List<SaldoDto>> CalcularSaldosAsync(int grupoId, int usuarioId, CancellationToken ct);
    Task<List<PagoPropuestoDto>> ProponerLiquidacionAsync(int grupoId, int usuarioId, CancellationToken ct);
    Task<LiquidacionDto> RegistrarLiquidacionAsync(int grupoId, RegistrarLiquidacionRequest request, int usuarioId, CancellationToken ct);

    /// <summary>
    /// Saldo de un miembro sin comprobar permisos del solicitante; lo usan otros servicios
    /// (p. ej. para impedir quitar del grupo a alguien con saldo pendiente).
    /// </summary>
    Task<decimal> ObtenerSaldoAsync(int grupoId, int usuarioId, CancellationToken ct);
}
