namespace GastosCompartidos.Api.Excepciones;

/// <summary>
/// El recurso solicitado no existe. Se traduce a un 404.
/// </summary>
public class NoEncontradoException(string mensaje) : Exception(mensaje);
