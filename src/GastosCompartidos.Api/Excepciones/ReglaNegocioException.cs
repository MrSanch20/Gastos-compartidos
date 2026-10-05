namespace GastosCompartidos.Api.Excepciones;

/// <summary>
/// Se ha violado una regla de negocio (p. ej. las participaciones no suman el importe total). Se traduce a un 400.
/// </summary>
public class ReglaNegocioException(string mensaje) : Exception(mensaje);
