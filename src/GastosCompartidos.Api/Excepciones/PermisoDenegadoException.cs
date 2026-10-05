namespace GastosCompartidos.Api.Excepciones;

/// <summary>
/// El usuario autenticado no tiene permiso para realizar la operación. Se traduce a un 403.
/// </summary>
public class PermisoDenegadoException(string mensaje) : Exception(mensaje);
