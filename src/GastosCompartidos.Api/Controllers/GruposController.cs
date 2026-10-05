using FluentValidation;
using GastosCompartidos.Api.DTOs.Grupos;
using GastosCompartidos.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GastosCompartidos.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/grupos")]
public class GruposController(
    IGrupoService grupoService,
    IValidator<CrearGrupoRequest> crearGrupoValidator,
    IValidator<AnadirMiembroRequest> anadirMiembroValidator) : ControladorBase
{
    [HttpGet]
    public async Task<ActionResult<List<GrupoResumenDto>>> ListarMisGrupos(CancellationToken ct)
    {
        var grupos = await grupoService.ListarMisGruposAsync(UsuarioActualId, ct);
        return Ok(grupos);
    }

    [HttpPost]
    public async Task<ActionResult<GrupoResumenDto>> Crear(CrearGrupoRequest request, CancellationToken ct)
    {
        await crearGrupoValidator.ValidateAndThrowAsync(request, ct);
        var grupo = await grupoService.CrearAsync(request, UsuarioActualId, ct);
        return CreatedAtAction(nameof(ObtenerDetalle), new { id = grupo.Id }, grupo);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GrupoDetalleDto>> ObtenerDetalle(int id, CancellationToken ct)
    {
        var grupo = await grupoService.ObtenerDetalleAsync(id, UsuarioActualId, ct);
        return Ok(grupo);
    }

    [HttpPost("{id:int}/miembros")]
    public async Task<IActionResult> AnadirMiembro(int id, AnadirMiembroRequest request, CancellationToken ct)
    {
        await anadirMiembroValidator.ValidateAndThrowAsync(request, ct);
        await grupoService.AnadirMiembroAsync(id, request, UsuarioActualId, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}/miembros/{usuarioId:int}")]
    public async Task<IActionResult> QuitarMiembro(int id, int usuarioId, CancellationToken ct)
    {
        await grupoService.QuitarMiembroAsync(id, usuarioId, UsuarioActualId, ct);
        return NoContent();
    }
}
