namespace GastosCompartidos.Api.DTOs.Auth;

public record AuthResponseDto(string Token, DateTime ExpiraEn, int UsuarioId, string Nombre, string Email);
