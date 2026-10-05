namespace GastosCompartidos.Api.Models;

/// <summary>
/// Conjunto de personas que comparten gastos (viaje, piso compartido, cena...).
/// </summary>
public class Grupo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }

    public ICollection<MiembroGrupo> Miembros { get; set; } = new List<MiembroGrupo>();
    public ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();
    public ICollection<Liquidacion> Liquidaciones { get; set; } = new List<Liquidacion>();
}
