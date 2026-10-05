namespace GastosCompartidos.Api.Excepciones;

/// <summary>
/// El email o la contraseña no son correctos. Se traduce a un 401.
/// </summary>
public class CredencialesInvalidasException(string mensaje) : Exception(mensaje);
