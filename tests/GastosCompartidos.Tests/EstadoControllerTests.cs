using System.Net;

namespace GastosCompartidos.Tests;

public class EstadoControllerTests(GastosCompartidosWebApplicationFactory fabrica) : IClassFixture<GastosCompartidosWebApplicationFactory>
{
    [Fact]
    public async Task ObtenerEstado_DevuelveOk()
    {
        var cliente = fabrica.CreateClient();

        var respuesta = await cliente.GetAsync("/api/estado");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }
}
