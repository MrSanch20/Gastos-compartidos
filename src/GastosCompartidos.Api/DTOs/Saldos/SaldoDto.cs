namespace GastosCompartidos.Api.DTOs.Saldos;

/// <summary>
/// Saldo neto de un miembro dentro del grupo: positivo significa que el grupo le debe dinero,
/// negativo que él es quien debe.
/// </summary>
public record SaldoDto(int UsuarioId, string Nombre, decimal Saldo);
