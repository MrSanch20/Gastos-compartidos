using FluentValidation;
using GastosCompartidos.Api.DTOs.Saldos;
using GastosCompartidos.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GastosCompartidos.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/grupos/{grupoId:int}")]
public class SaldosController(
    ISaldoService saldoService,
    IValidator<RegistrarLiquidacionRequest> registrarLiquidacionValidator) : ControladorBase
{
    [HttpGet("saldos")]
    public async Task<ActionResult<List<SaldoDto>>> ObtenerSaldos(int grupoId, CancellationToken ct)
    {
        var saldos = await saldoService.CalcularSaldosAsync(grupoId, UsuarioActualId, ct);
        return Ok(saldos);
    }

    [HttpGet("saldar")]
    public async Task<ActionResult<List<PagoPropuestoDto>>> ProponerSaldar(int grupoId, CancellationToken ct)
    {
        var pagos = await saldoService.ProponerLiquidacionAsync(grupoId, UsuarioActualId, ct);
        return Ok(pagos);
    }

    [HttpPost("liquidaciones")]
    public async Task<ActionResult<LiquidacionDto>> RegistrarLiquidacion(
        int grupoId, RegistrarLiquidacionRequest request, CancellationToken ct)
    {
        await registrarLiquidacionValidator.ValidateAndThrowAsync(request, ct);
        var liquidacion = await saldoService.RegistrarLiquidacionAsync(grupoId, request, UsuarioActualId, ct);
        return CreatedAtAction(nameof(ObtenerSaldos), new { grupoId }, liquidacion);
    }
}
