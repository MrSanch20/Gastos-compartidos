using System.Net;
using System.Net.Http.Json;
using GastosCompartidos.Api.DTOs.Gastos;
using GastosCompartidos.Api.DTOs.Saldos;
using GastosCompartidos.Api.Models;

namespace GastosCompartidos.Tests;

public class SaldosControllerTests(GastosCompartidosWebApplicationFactory fabrica) : IClassFixture<GastosCompartidosWebApplicationFactory>
{
    [Fact]
    public async Task ObtenerSaldos_TrasUnGastoAPartesIguales_DevuelveElSaldoEsperado()
    {
        var (grupo, participantes) = await EscenarioTestHelper.CrearGrupoConMiembrosAsync(fabrica, 2, "SaldoHttp");
        var admin = participantes[0];
        var otro = participantes[1];

        await admin.Cliente.PostAsJsonAsync($"/api/grupos/{grupo.Id}/gastos", new CrearGastoRequest(
            "Cena", 20m, DateTime.UtcNow, admin.Auth.UsuarioId, TipoReparto.PartesIguales,
            [new ParticipacionRequest(admin.Auth.UsuarioId, null), new ParticipacionRequest(otro.Auth.UsuarioId, null)]));

        var respuesta = await admin.Cliente.GetAsync($"/api/grupos/{grupo.Id}/saldos");
        var saldos = (await respuesta.Content.ReadFromJsonAsync<List<SaldoDto>>())!;

        Assert.Equal(10m, saldos.Single(s => s.UsuarioId == admin.Auth.UsuarioId).Saldo);
        Assert.Equal(-10m, saldos.Single(s => s.UsuarioId == otro.Auth.UsuarioId).Saldo);
    }

    [Fact]
    public async Task ProponerSaldar_DespuesDeRegistrarLaLiquidacionPropuesta_TodosQuedanEnPaz()
    {
        var (grupo, participantes) = await EscenarioTestHelper.CrearGrupoConMiembrosAsync(fabrica, 2, "SaldarHttp");
        var admin = participantes[0];
        var otro = participantes[1];

        await admin.Cliente.PostAsJsonAsync($"/api/grupos/{grupo.Id}/gastos", new CrearGastoRequest(
            "Alquiler", 30m, DateTime.UtcNow, admin.Auth.UsuarioId, TipoReparto.PartesIguales,
            [new ParticipacionRequest(admin.Auth.UsuarioId, null), new ParticipacionRequest(otro.Auth.UsuarioId, null)]));

        var propuesta = (await (await admin.Cliente.GetAsync($"/api/grupos/{grupo.Id}/saldar"))
            .Content.ReadFromJsonAsync<List<PagoPropuestoDto>>())!;
        Assert.Single(propuesta);

        var pago = propuesta[0];
        var respuestaRegistro = await admin.Cliente.PostAsJsonAsync(
            $"/api/grupos/{grupo.Id}/liquidaciones",
            new RegistrarLiquidacionRequest(pago.DeId, pago.ParaId, pago.Importe, null));
        Assert.Equal(HttpStatusCode.Created, respuestaRegistro.StatusCode);

        var saldosFinales = (await (await admin.Cliente.GetAsync($"/api/grupos/{grupo.Id}/saldos"))
            .Content.ReadFromJsonAsync<List<SaldoDto>>())!;
        Assert.All(saldosFinales, s => Assert.Equal(0m, s.Saldo));

        var propuestaFinal = (await (await admin.Cliente.GetAsync($"/api/grupos/{grupo.Id}/saldar"))
            .Content.ReadFromJsonAsync<List<PagoPropuestoDto>>())!;
        Assert.Empty(propuestaFinal);
    }

    [Fact]
    public async Task RegistrarLiquidacion_ConUsuarioQueNoEsMiembro_DevuelveSolicitudInvalida()
    {
        var (grupo, participantes) = await EscenarioTestHelper.CrearGrupoConMiembrosAsync(fabrica, 1, "LiqAjeno");
        var admin = participantes[0];
        var ajeno = await AuthTestHelper.RegistrarAsync(fabrica.CreateClient(), "AjenoLiq");

        var respuesta = await admin.Cliente.PostAsJsonAsync(
            $"/api/grupos/{grupo.Id}/liquidaciones",
            new RegistrarLiquidacionRequest(admin.Auth.UsuarioId, ajeno.UsuarioId, 10m, null));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task ObtenerSaldos_SiNoSoyMiembro_DevuelveProhibido()
    {
        var (grupo, _) = await EscenarioTestHelper.CrearGrupoConMiembrosAsync(fabrica, 1, "SaldoProhibido");
        var intruso = fabrica.CreateClient();
        var auth = await AuthTestHelper.RegistrarAsync(intruso, "IntrusoSaldo");
        AuthTestHelper.AutenticarComo(intruso, auth);

        var respuesta = await intruso.GetAsync($"/api/grupos/{grupo.Id}/saldos");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task QuitarMiembro_ConSaldoPendiente_DevuelveConflicto()
    {
        var (grupo, participantes) = await EscenarioTestHelper.CrearGrupoConMiembrosAsync(fabrica, 2, "QuitarConDeuda");
        var admin = participantes[0];
        var deudor = participantes[1];

        await admin.Cliente.PostAsJsonAsync($"/api/grupos/{grupo.Id}/gastos", new CrearGastoRequest(
            "Cena", 20m, DateTime.UtcNow, admin.Auth.UsuarioId, TipoReparto.PartesIguales,
            [new ParticipacionRequest(admin.Auth.UsuarioId, null), new ParticipacionRequest(deudor.Auth.UsuarioId, null)]));

        var respuesta = await admin.Cliente.DeleteAsync($"/api/grupos/{grupo.Id}/miembros/{deudor.Auth.UsuarioId}");

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task QuitarMiembro_TrasSaldarSuDeuda_SePuedeQuitarSinProblema()
    {
        var (grupo, participantes) = await EscenarioTestHelper.CrearGrupoConMiembrosAsync(fabrica, 2, "QuitarSinDeuda");
        var admin = participantes[0];
        var miembro = participantes[1];

        await admin.Cliente.PostAsJsonAsync($"/api/grupos/{grupo.Id}/gastos", new CrearGastoRequest(
            "Cena", 20m, DateTime.UtcNow, admin.Auth.UsuarioId, TipoReparto.PartesIguales,
            [new ParticipacionRequest(admin.Auth.UsuarioId, null), new ParticipacionRequest(miembro.Auth.UsuarioId, null)]));

        await admin.Cliente.PostAsJsonAsync(
            $"/api/grupos/{grupo.Id}/liquidaciones",
            new RegistrarLiquidacionRequest(miembro.Auth.UsuarioId, admin.Auth.UsuarioId, 10m, null));

        var respuesta = await admin.Cliente.DeleteAsync($"/api/grupos/{grupo.Id}/miembros/{miembro.Auth.UsuarioId}");

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
    }
}
