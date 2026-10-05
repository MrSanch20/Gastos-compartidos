namespace GastosCompartidos.Api.Models;

/// <summary>
/// Pago de un miembro a otro para saldar deudas dentro de un grupo.
/// </summary>
public class Liquidacion
{
    public int Id { get; set; }

    public int GrupoId { get; set; }
    public Grupo Grupo { get; set; } = null!;

    public int DeId { get; set; }
    public Usuario De { get; set; } = null!;

    public int ParaId { get; set; }
    public Usuario Para { get; set; } = null!;

    public decimal Importe { get; set; }
    public DateTime Fecha { get; set; }
}
