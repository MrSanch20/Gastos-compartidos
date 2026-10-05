namespace GastosCompartidos.Api.DTOs.Saldos;

public record LiquidacionDto(int Id, int GrupoId, int DeId, string DeNombre, int ParaId, string ParaNombre, decimal Importe, DateTime Fecha);
