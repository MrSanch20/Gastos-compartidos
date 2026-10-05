namespace GastosCompartidos.Api.DTOs.Gastos;

public record GastoDto(
    int Id,
    int GrupoId,
    int PagadoPorId,
    string PagadoPorNombre,
    decimal Importe,
    string Concepto,
    DateTime Fecha,
    List<ParticipacionDto> Participaciones);
