using System.Net;
using System.Net.Http.Json;
using GastosCompartidos.Api.DTOs.Auth;

namespace GastosCompartidos.Tests;

public class AuthControllerTests(GastosCompartidosWebApplicationFactory fabrica) : IClassFixture<GastosCompartidosWebApplicationFactory>
{
    private readonly HttpClient _cliente = fabrica.CreateClient();

    [Fact]
    public async Task Registro_ConDatosValidos_DevuelveToken()
    {
        var request = new RegistroRequest("Ana", $"ana-{Guid.NewGuid()}@test.com", "ContrasenaSegura123");

        var respuesta = await _cliente.PostAsJsonAsync("/api/auth/registro", request);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var body = await respuesta.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
    }

    [Fact]
    public async Task Registro_ConEmailYaRegistrado_DevuelveConflicto()
    {
        var email = $"duplicado-{Guid.NewGuid()}@test.com";
        var request = new RegistroRequest("Bea", email, "ContrasenaSegura123");

        var primeraRespuesta = await _cliente.PostAsJsonAsync("/api/auth/registro", request);
        Assert.Equal(HttpStatusCode.OK, primeraRespuesta.StatusCode);

        var segundaRespuesta = await _cliente.PostAsJsonAsync("/api/auth/registro", request);

        Assert.Equal(HttpStatusCode.Conflict, segundaRespuesta.StatusCode);
    }

    [Fact]
    public async Task Login_ConContrasenaIncorrecta_DevuelveNoAutorizado()
    {
        var email = $"login-{Guid.NewGuid()}@test.com";
        await _cliente.PostAsJsonAsync("/api/auth/registro", new RegistroRequest("Carla", email, "ContrasenaCorrecta123"));

        var respuesta = await _cliente.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "ContrasenaIncorrecta"));

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Login_ConEmailNoRegistrado_DevuelveNoAutorizado()
    {
        var respuesta = await _cliente.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest($"noexiste-{Guid.NewGuid()}@test.com", "CualquierContrasena123"));

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Login_ConCredencialesCorrectas_DevuelveToken()
    {
        var email = $"ok-{Guid.NewGuid()}@test.com";
        const string contrasena = "ContrasenaCorrecta123";
        await _cliente.PostAsJsonAsync("/api/auth/registro", new RegistroRequest("Dani", email, contrasena));

        var respuesta = await _cliente.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, contrasena));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var body = await respuesta.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
    }
}
