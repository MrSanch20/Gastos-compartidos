namespace GastosCompartidos.Api.DTOs.Grupos;

public record GrupoDetalleDto(int Id, string Nombre, DateTime FechaCreacion, List<MiembroDto> Miembros);
