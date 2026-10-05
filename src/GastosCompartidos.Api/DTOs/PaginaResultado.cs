namespace GastosCompartidos.Api.DTOs;

public record PaginaResultado<T>(List<T> Elementos, int TotalElementos, int Pagina, int TamanoPagina);
