namespace GastosCompartidos.Api.Models;

/// <summary>
/// Relación entre un usuario y un grupo del que forma parte.
/// </summary>
public class MiembroGrupo
{
    public int GrupoId { get; set; }
    public Grupo Grupo { get; set; } = null!;

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public RolGrupo Rol { get; set; }
}
