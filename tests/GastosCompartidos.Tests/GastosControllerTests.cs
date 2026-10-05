using System.Net;
using System.Net.Http.Json;
using GastosCompartidos.Api.DTOs;
using GastosCompartidos.Api.DTOs.Gastos;
using GastosCompartidos.Api.Models;

namespace GastosCompartidos.Tests;

public class GastosControllerTests(GastosCompartidosWebApplicationFactory fabrica) : IClassFixture<GastosCompartidosWebApplicationFactory>
{
    [Fact]
    public async Task Crear_ConPartesIguales_RepartenLosCentimosDeFormaJusta()
    {
        var (grupo, participantes) = await EscenarioTestHelper.CrearGrupoConMiembrosAsync(fabrica, 3, "GastoIgual");
        var admin = participantes[0];

        var request = new CrearGastoRequest(
            "Cena", 10m, DateTime.UtcNow, admin.Auth.UsuarioId, TipoReparto.PartesIguales,
            participantes.Select(p => new ParticipacionRequest(p.Auth.UsuarioId, null)).ToList());

        var respuesta = await admin.Cliente.PostAsJsonAsync($"/api/grupos/{grupo.Id}/gastos", request);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var gasto = (await respuesta.Content.ReadFromJsonAsync<GastoDto>())!;
        Assert.Equal(10m, gasto.Participaciones.Sum(p => p.Importe));
        Assert.Contains(gasto.Participaciones, p => p.Importe == 3.34m);
    }

    [Fact]
    public async Task Crear_PersonalizadoConSumaDistintaAlTotal_DevuelveSolicitudInvalida()
    {
        var (grupo, participantes) = await EscenarioTestHelper.CrearGrupoConMiembrosAsync(fabrica, 2, "GastoMalSuma");
        var admin = participantes[0];

        var request = new CrearGastoRequest(
            "Compra", 10m, DateTime.UtcNow, admin.Auth.UsuarioId, TipoReparto.Personalizado,
            [
                new ParticipacionRequest(participantes[0].Auth.UsuarioId, 4m),
                new ParticipacionRequest(participantes[1].Auth.UsuarioId, 4m)
            ]);

        var respuesta = await admin.Cliente.PostAsJsonAsync($"/api/grupos/{grupo.Id}/gastos", request);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_PersonalizadoConSumaCorrecta_SeCreaConLosImportesIndicados()
    {
        var (grupo, participantes) = await EscenarioTestHelper.CrearGrupoConMiembrosAsync(fabrica, 2, "GastoBienSuma");
        var admin = participantes[0];

        var request = new CrearGastoRequest(
            "Compra", 10m, DateTime.UtcNow, admin.Auth.UsuarioId, TipoReparto.Personalizado,
            [
                new ParticipacionRequest(participantes[0].Auth.UsuarioId, 7m),
                new ParticipacionRequest(participantes[1].Auth.UsuarioId, 3m)
            ]);

        var respuesta = await admin.Cliente.PostAsJsonAsync($"/api/grupos/{grupo.Id}/gastos", request);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_ConParticipanteQueNoEsMiembro_DevuelveSolicitudInvalida()
    {
        var (grupo, participantes) = await EscenarioTestHelper.CrearGrupoConMiembrosAsync(fabrica, 1, "GastoAjeno");
        var admin = participantes[0];
        var ajeno = await AuthTestHelper.RegistrarAsync(fabrica.CreateClient(), "Ajeno1");

        var request = new CrearGastoRequest(
            "Cena", 10m, DateTime.UtcNow, admin.Auth.UsuarioId, TipoReparto.PartesIguales,
            [new ParticipacionRequest(admin.Auth.UsuarioId, null), new ParticipacionRequest(ajeno.UsuarioId, null)]);

        var respuesta = await admin.Cliente.PostAsJsonAsync($"/api/grupos/{grupo.Id}/gastos", request);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Listar_FiltraPorRangoDeFechasYPagina()
    {
        var (grupo, participantes) = await EscenarioTestHelper.CrearGrupoConMiembrosAsync(fabrica, 1, "GastoListar");
        var admin = participantes[0];
        var participacionesIguales = new List<ParticipacionRequest> { new(admin.Auth.UsuarioId, null) };

        var fechaAntigua = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var fechaReciente = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        await admin.Cliente.PostAsJsonAsync($"/api/grupos/{grupo.Id}/gastos",
            new CrearGastoRequest("Antiguo", 5m, fechaAntigua, admin.Auth.UsuarioId, TipoReparto.PartesIguales, participacionesIguales));
        await admin.Cliente.PostAsJsonAsync($"/api/grupos/{grupo.Id}/gastos",
            new CrearGastoRequest("Reciente", 7m, fechaReciente, admin.Auth.UsuarioId, TipoReparto.PartesIguales, participacionesIguales));

        var respuesta = await admin.Cliente.GetAsync(
            $"/api/grupos/{grupo.Id}/gastos?fechaDesde=2025-01-01&pagina=1&tamanoPagina=10");
        var pagina = (await respuesta.Content.ReadFromJsonAsync<PaginaResultado<GastoDto>>())!;

        Assert.Single(pagina.Elementos);
        Assert.Equal("Reciente", pagina.Elementos[0].Concepto);
    }

    [Fact]
    public async Task EditarYBorrar_FuncionanParaCualquierMiembro()
    {
        var (grupo, participantes) = await EscenarioTestHelper.CrearGrupoConMiembrosAsync(fabrica, 2, "GastoEditar");
        var admin = participantes[0];
        var otroMiembro = participantes[1];

        var creado = (await (await admin.Cliente.PostAsJsonAsync($"/api/grupos/{grupo.Id}/gastos",
            new CrearGastoRequest("Original", 10m, DateTime.UtcNow, admin.Auth.UsuarioId, TipoReparto.PartesIguales,
                [new ParticipacionRequest(admin.Auth.UsuarioId, null)])))
            .Content.ReadFromJsonAsync<GastoDto>())!;

        var respuestaEditar = await otroMiembro.Cliente.PutAsJsonAsync(
            $"/api/grupos/{grupo.Id}/gastos/{creado.Id}",
            new ActualizarGastoRequest("Editado", 20m, DateTime.UtcNow, admin.Auth.UsuarioId, TipoReparto.PartesIguales,
                [new ParticipacionRequest(admin.Auth.UsuarioId, null)]));
        Assert.Equal(HttpStatusCode.OK, respuestaEditar.StatusCode);
        var editado = (await respuestaEditar.Content.ReadFromJsonAsync<GastoDto>())!;
        Assert.Equal("Editado", editado.Concepto);
        Assert.Equal(20m, editado.Importe);

        var respuestaBorrar = await otroMiembro.Cliente.DeleteAsync($"/api/grupos/{grupo.Id}/gastos/{creado.Id}");
        Assert.Equal(HttpStatusCode.NoContent, respuestaBorrar.StatusCode);

        var respuestaListarTrasBorrar = await admin.Cliente.GetAsync($"/api/grupos/{grupo.Id}/gastos");
        var paginaTrasBorrar = (await respuestaListarTrasBorrar.Content.ReadFromJsonAsync<PaginaResultado<GastoDto>>())!;
        Assert.Empty(paginaTrasBorrar.Elementos);
    }

    [Fact]
    public async Task Listar_SiNoSoyMiembro_DevuelveProhibido()
    {
        var (grupo, _) = await EscenarioTestHelper.CrearGrupoConMiembrosAsync(fabrica, 1, "GastoProhibido");
        var intruso = fabrica.CreateClient();
        var auth = await AuthTestHelper.RegistrarAsync(intruso, "IntrusoGasto");
        AuthTestHelper.AutenticarComo(intruso, auth);

        var respuesta = await intruso.GetAsync($"/api/grupos/{grupo.Id}/gastos");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }
}
