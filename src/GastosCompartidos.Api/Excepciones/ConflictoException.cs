namespace GastosCompartidos.Api.Excepciones;

/// <summary>
/// La operación entra en conflicto con el estado actual de los datos (p. ej. email duplicado). Se traduce a un 409.
/// </summary>
public class ConflictoException(string mensaje) : Exception(mensaje);
