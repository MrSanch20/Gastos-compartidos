using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GastosCompartidos.Tests;

public class EstadoControllerTests(WebApplicationFactory<Program> fabrica) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task ObtenerEstado_DevuelveOk()
    {
        var cliente = fabrica.CreateClient();

        var respuesta = await cliente.GetAsync("/api/estado");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }
}
