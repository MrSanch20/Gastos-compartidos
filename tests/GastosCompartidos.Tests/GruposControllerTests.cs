using System.Net;
using System.Net.Http.Json;
using GastosCompartidos.Api.DTOs.Grupos;

namespace GastosCompartidos.Tests;

public class GruposControllerTests(GastosCompartidosWebApplicationFactory fabrica) : IClassFixture<GastosCompartidosWebApplicationFactory>
{
    [Fact]
    public async Task Crear_DejaAlCreadorComoAdminYMiembroUnico()
    {
        var cliente = fabrica.CreateClient();
        var autor = await AuthTestHelper.RegistrarAsync(cliente, "Admin1");
        AuthTestHelper.AutenticarComo(cliente, autor);

        var respuestaCrear = await cliente.PostAsJsonAsync("/api/grupos", new CrearGrupoRequest("Viaje a Roma"));
        Assert.Equal(HttpStatusCode.Created, respuestaCrear.StatusCode);
        var grupo = (await respuestaCrear.Content.ReadFromJsonAsync<GrupoResumenDto>())!;

        var respuestaDetalle = await cliente.GetAsync($"/api/grupos/{grupo.Id}");
        var detalle = (await respuestaDetalle.Content.ReadFromJsonAsync<GrupoDetalleDto>())!;

        Assert.Single(detalle.Miembros);
        Assert.Equal(autor.UsuarioId, detalle.Miembros[0].UsuarioId);
        Assert.Equal(Api.Models.RolGrupo.Admin, detalle.Miembros[0].Rol);
    }

    [Fact]
    public async Task ListarMisGrupos_SoloIncluyeLosGruposDondeSoyMiembro()
    {
        var clienteA = fabrica.CreateClient();
        var usuarioA = await AuthTestHelper.RegistrarAsync(clienteA, "UsuarioA");
        AuthTestHelper.AutenticarComo(clienteA, usuarioA);
        await clienteA.PostAsJsonAsync("/api/grupos", new CrearGrupoRequest("Grupo de A"));

        var clienteB = fabrica.CreateClient();
        var usuarioB = await AuthTestHelper.RegistrarAsync(clienteB, "UsuarioB");
        AuthTestHelper.AutenticarComo(clienteB, usuarioB);

        var respuesta = await clienteB.GetAsync("/api/grupos");
        var grupos = (await respuesta.Content.ReadFromJsonAsync<List<GrupoResumenDto>>())!;

        Assert.DoesNotContain(grupos, g => g.Nombre == "Grupo de A");
    }

    [Fact]
    public async Task ObtenerDetalle_SiNoSoyMiembro_DevuelveProhibido()
    {
        var clienteCreador = fabrica.CreateClient();
        var creador = await AuthTestHelper.RegistrarAsync(clienteCreador, "Creador1");
        AuthTestHelper.AutenticarComo(clienteCreador, creador);
        var grupo = (await (await clienteCreador.PostAsJsonAsync("/api/grupos", new CrearGrupoRequest("Grupo privado")))
            .Content.ReadFromJsonAsync<GrupoResumenDto>())!;

        var clienteIntruso = fabrica.CreateClient();
        var intruso = await AuthTestHelper.RegistrarAsync(clienteIntruso, "Intruso1");
        AuthTestHelper.AutenticarComo(clienteIntruso, intruso);

        var respuesta = await clienteIntruso.GetAsync($"/api/grupos/{grupo.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task AnadirMiembro_SiQuienLoHaceNoEsAdmin_DevuelveProhibido()
    {
        var clienteAdmin = fabrica.CreateClient();
        var admin = await AuthTestHelper.RegistrarAsync(clienteAdmin, "Admin2");
        AuthTestHelper.AutenticarComo(clienteAdmin, admin);
        var grupo = (await (await clienteAdmin.PostAsJsonAsync("/api/grupos", new CrearGrupoRequest("Grupo con miembros")))
            .Content.ReadFromJsonAsync<GrupoResumenDto>())!;

        var clienteMiembro = fabrica.CreateClient();
        var miembro = await AuthTestHelper.RegistrarAsync(clienteMiembro, "Miembro1");
        AuthTestHelper.AutenticarComo(clienteMiembro, miembro);
        await clienteAdmin.PostAsJsonAsync($"/api/grupos/{grupo.Id}/miembros", new AnadirMiembroRequest(miembro.Email));

        var terceraPersona = await AuthTestHelper.RegistrarAsync(fabrica.CreateClient(), "Tercero1");

        var respuesta = await clienteMiembro.PostAsJsonAsync(
            $"/api/grupos/{grupo.Id}/miembros",
            new AnadirMiembroRequest(terceraPersona.Email));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task AnadirMiembro_SiYaEsMiembro_DevuelveConflicto()
    {
        var clienteAdmin = fabrica.CreateClient();
        var admin = await AuthTestHelper.RegistrarAsync(clienteAdmin, "Admin3");
        AuthTestHelper.AutenticarComo(clienteAdmin, admin);
        var grupo = (await (await clienteAdmin.PostAsJsonAsync("/api/grupos", new CrearGrupoRequest("Grupo duplicado")))
            .Content.ReadFromJsonAsync<GrupoResumenDto>())!;

        var nuevoMiembro = await AuthTestHelper.RegistrarAsync(fabrica.CreateClient(), "NuevoMiembro1");
        await clienteAdmin.PostAsJsonAsync($"/api/grupos/{grupo.Id}/miembros", new AnadirMiembroRequest(nuevoMiembro.Email));

        var respuesta = await clienteAdmin.PostAsJsonAsync(
            $"/api/grupos/{grupo.Id}/miembros",
            new AnadirMiembroRequest(nuevoMiembro.Email));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task QuitarMiembro_SiQuienLoHaceNoEsAdmin_DevuelveProhibido()
    {
        var clienteAdmin = fabrica.CreateClient();
        var admin = await AuthTestHelper.RegistrarAsync(clienteAdmin, "Admin4");
        AuthTestHelper.AutenticarComo(clienteAdmin, admin);
        var grupo = (await (await clienteAdmin.PostAsJsonAsync("/api/grupos", new CrearGrupoRequest("Grupo para quitar")))
            .Content.ReadFromJsonAsync<GrupoResumenDto>())!;

        var clienteMiembro = fabrica.CreateClient();
        var miembro = await AuthTestHelper.RegistrarAsync(clienteMiembro, "Miembro2");
        AuthTestHelper.AutenticarComo(clienteMiembro, miembro);
        await clienteAdmin.PostAsJsonAsync($"/api/grupos/{grupo.Id}/miembros", new AnadirMiembroRequest(miembro.Email));

        var respuesta = await clienteMiembro.DeleteAsync($"/api/grupos/{grupo.Id}/miembros/{admin.UsuarioId}");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }
}
