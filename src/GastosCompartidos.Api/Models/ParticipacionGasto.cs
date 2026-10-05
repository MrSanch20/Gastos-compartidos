namespace GastosCompartidos.Api.Models;

/// <summary>
/// Parte de un gasto que corresponde a un miembro concreto.
/// </summary>
public class ParticipacionGasto
{
    public int GastoId { get; set; }
    public Gasto Gasto { get; set; } = null!;

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public decimal Importe { get; set; }
}
