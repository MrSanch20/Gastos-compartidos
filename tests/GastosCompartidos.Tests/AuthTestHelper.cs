using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GastosCompartidos.Api.DTOs.Auth;

namespace GastosCompartidos.Tests;

/// <summary>
/// Registra usuarios de prueba y deja el cliente HTTP listo con el token JWT puesto,
/// para no repetir el mismo código de registro/login en cada clase de tests.
/// </summary>
public static class AuthTestHelper
{
    /// <summary>
    /// La API serializa los enums como texto (ver Program.cs); el HttpClient de los tests
    /// necesita este mismo convertidor para poder deserializar esas respuestas.
    /// </summary>
    public static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<AuthResponseDto> RegistrarAsync(HttpClient cliente, string nombre, string? email = null)
    {
        var request = new RegistroRequest(nombre, email ?? $"{nombre.ToLowerInvariant()}-{Guid.NewGuid()}@test.com", "ContrasenaSegura123");
        var respuesta = await cliente.PostAsJsonAsync("/api/auth/registro", request);
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<AuthResponseDto>())!;
    }

    public static void AutenticarComo(HttpClient cliente, AuthResponseDto auth)
    {
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
    }
}
