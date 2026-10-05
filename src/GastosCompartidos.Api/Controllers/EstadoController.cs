using Microsoft.AspNetCore.Mvc;

namespace GastosCompartidos.Api.Controllers;

/// <summary>
/// Endpoint de prueba para comprobar que la API está en marcha.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EstadoController : ControllerBase
{
    [HttpGet]
    public IActionResult ObtenerEstado()
    {
        return Ok(new { estado = "ok", fecha = DateTime.UtcNow });
    }
}
