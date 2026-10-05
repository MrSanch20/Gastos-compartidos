namespace GastosCompartidos.Api.Models;

/// <summary>
/// Persona registrada en la aplicación.
/// </summary>
public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<MiembroGrupo> GruposPertenecientes { get; set; } = new List<MiembroGrupo>();
    public ICollection<Gasto> GastosPagados { get; set; } = new List<Gasto>();
    public ICollection<ParticipacionGasto> Participaciones { get; set; } = new List<ParticipacionGasto>();
}
