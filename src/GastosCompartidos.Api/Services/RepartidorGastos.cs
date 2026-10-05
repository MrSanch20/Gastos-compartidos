namespace GastosCompartidos.Api.Services;

/// <summary>
/// Reparte un importe a partes iguales entre varios participantes, asignando los céntimos
/// sobrantes de forma justa (10 € entre 3 → 3,34 + 3,33 + 3,33) en vez de perderlos por redondeo.
/// </summary>
public static class RepartidorGastos
{
    public static List<(int UsuarioId, decimal Importe)> RepartirPartesIguales(decimal importeTotal, IReadOnlyList<int> usuarioIds)
    {
        if (usuarioIds.Count == 0)
        {
            throw new ArgumentException("Debe haber al menos un participante.", nameof(usuarioIds));
        }

        var centimosTotales = (long)Math.Round(importeTotal * 100m, MidpointRounding.AwayFromZero);
        var numeroParticipantes = usuarioIds.Count;
        var centimosBase = centimosTotales / numeroParticipantes;
        var centimosSobrantes = centimosTotales % numeroParticipantes;

        var resultado = new List<(int UsuarioId, decimal Importe)>(numeroParticipantes);
        for (var i = 0; i < numeroParticipantes; i++)
        {
            var centimos = centimosBase + (i < centimosSobrantes ? 1 : 0);
            resultado.Add((usuarioIds[i], centimos / 100m));
        }

        return resultado;
    }
}
