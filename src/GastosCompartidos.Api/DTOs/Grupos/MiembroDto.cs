using GastosCompartidos.Api.Models;

namespace GastosCompartidos.Api.DTOs.Grupos;

public record MiembroDto(int UsuarioId, string Nombre, string Email, RolGrupo Rol);
