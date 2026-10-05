namespace GastosCompartidos.Api.DTOs.Gastos;

/// <summary>
/// Participante de un gasto. El importe solo es obligatorio cuando el reparto es personalizado;
/// en un reparto a partes iguales se ignora y se recalcula.
/// </summary>
public record ParticipacionRequest(int UsuarioId, decimal? Importe);
