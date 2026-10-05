using FluentValidation;
using GastosCompartidos.Api.DTOs;
using GastosCompartidos.Api.DTOs.Gastos;
using GastosCompartidos.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GastosCompartidos.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/grupos/{grupoId:int}/gastos")]
public class GastosController(
    IGastoService gastoService,
    IValidator<CrearGastoRequest> crearGastoValidator,
    IValidator<ActualizarGastoRequest> actualizarGastoValidator) : ControladorBase
{
    [HttpGet]
    public async Task<ActionResult<PaginaResultado<GastoDto>>> Listar(
        int grupoId,
        CancellationToken ct,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        var resultado = await gastoService.ListarAsync(
            grupoId, UsuarioActualId, Math.Max(pagina, 1), Math.Clamp(tamanoPagina, 1, 100), fechaDesde, fechaHasta, ct);
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<ActionResult<GastoDto>> Crear(int grupoId, CrearGastoRequest request, CancellationToken ct)
    {
        await crearGastoValidator.ValidateAndThrowAsync(request, ct);
        var gasto = await gastoService.CrearAsync(grupoId, request, UsuarioActualId, ct);
        return CreatedAtAction(nameof(Listar), new { grupoId }, gasto);
    }

    [HttpPut("{gastoId:int}")]
    public async Task<ActionResult<GastoDto>> Editar(int grupoId, int gastoId, ActualizarGastoRequest request, CancellationToken ct)
    {
        await actualizarGastoValidator.ValidateAndThrowAsync(request, ct);
        var gasto = await gastoService.EditarAsync(grupoId, gastoId, request, UsuarioActualId, ct);
        return Ok(gasto);
    }

    [HttpDelete("{gastoId:int}")]
    public async Task<IActionResult> Borrar(int grupoId, int gastoId, CancellationToken ct)
    {
        await gastoService.BorrarAsync(grupoId, gastoId, UsuarioActualId, ct);
        return NoContent();
    }
}
