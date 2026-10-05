namespace GastosCompartidos.Api.Models;

/// <summary>
/// Algo que un miembro pagó en nombre del grupo.
/// </summary>
public class Gasto
{
    public int Id { get; set; }

    public int GrupoId { get; set; }
    public Grupo Grupo { get; set; } = null!;

    public int PagadoPorId { get; set; }
    public Usuario PagadoPor { get; set; } = null!;

    public decimal Importe { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }

    public ICollection<ParticipacionGasto> Participaciones { get; set; } = new List<ParticipacionGasto>();
}
