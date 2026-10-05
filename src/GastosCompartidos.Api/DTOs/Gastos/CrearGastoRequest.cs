using GastosCompartidos.Api.Models;

namespace GastosCompartidos.Api.DTOs.Gastos;

public record CrearGastoRequest(
    string Concepto,
    decimal Importe,
    DateTime Fecha,
    int PagadoPorId,
    TipoReparto TipoReparto,
    List<ParticipacionRequest> Participaciones);
