using System.Net.Http.Json;
using GastosCompartidos.Api.DTOs.Auth;
using GastosCompartidos.Api.DTOs.Grupos;

namespace GastosCompartidos.Tests;

/// <summary>
/// Monta un grupo con varios miembros ya autenticados, para no repetir el mismo
/// registro + login + alta de miembros en cada test de gastos y saldos.
/// </summary>
public static class EscenarioTestHelper
{
    public record Participante(HttpClient Cliente, AuthResponseDto Auth);

    public static async Task<(GrupoResumenDto Grupo, List<Participante> Participantes)> CrearGrupoConMiembrosAsync(
        GastosCompartidosWebApplicationFactory fabrica, int numeroMiembros, string prefijo)
    {
        var participantes = new List<Participante>();
        for (var i = 0; i < numeroMiembros; i++)
        {
            var cliente = fabrica.CreateClient();
            var auth = await AuthTestHelper.RegistrarAsync(cliente, $"{prefijo}{i}");
            AuthTestHelper.AutenticarComo(cliente, auth);
            participantes.Add(new Participante(cliente, auth));
        }

        var admin = participantes[0];
        var respuestaGrupo = await admin.Cliente.PostAsJsonAsync("/api/grupos", new CrearGrupoRequest($"{prefijo}-grupo"));
        var grupo = (await respuestaGrupo.Content.ReadFromJsonAsync<GrupoResumenDto>())!;

        foreach (var participante in participantes.Skip(1))
        {
            await admin.Cliente.PostAsJsonAsync($"/api/grupos/{grupo.Id}/miembros", new AnadirMiembroRequest(participante.Auth.Email));
        }

        return (grupo, participantes);
    }
}
