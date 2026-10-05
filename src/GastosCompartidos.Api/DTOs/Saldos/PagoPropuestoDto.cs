namespace GastosCompartidos.Api.DTOs.Saldos;

public record PagoPropuestoDto(int DeId, string DeNombre, int ParaId, string ParaNombre, decimal Importe);
