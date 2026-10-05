namespace GastosCompartidos.Api.DTOs.Saldos;

public record RegistrarLiquidacionRequest(int DeId, int ParaId, decimal Importe, DateTime? Fecha);
