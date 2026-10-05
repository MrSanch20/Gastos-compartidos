using System.Net;
using FluentValidation;
using GastosCompartidos.Api.Excepciones;
using Microsoft.AspNetCore.Mvc;

namespace GastosCompartidos.Api.Middleware;

/// <summary>
/// Captura las excepciones no controladas de toda la aplicación y las convierte en una
/// respuesta ProblemDetails homogénea, en vez de dejar que cada controller gestione sus propios errores.
/// </summary>
public class ManejadorExcepcionesMiddleware(RequestDelegate siguiente, ILogger<ManejadorExcepcionesMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await siguiente(contexto);
        }
        catch (Exception excepcion)
        {
            await ManejarExcepcionAsync(contexto, excepcion);
        }
    }

    private async Task ManejarExcepcionAsync(HttpContext contexto, Exception excepcion)
    {
        var (codigoEstado, titulo) = excepcion switch
        {
            ValidationException => (HttpStatusCode.BadRequest, "Error de validación"),
            ReglaNegocioException => (HttpStatusCode.BadRequest, "Solicitud inválida"),
            CredencialesInvalidasException => (HttpStatusCode.Unauthorized, "No autenticado"),
            PermisoDenegadoException => (HttpStatusCode.Forbidden, "Acceso denegado"),
            NoEncontradoException => (HttpStatusCode.NotFound, "Recurso no encontrado"),
            ConflictoException => (HttpStatusCode.Conflict, "Conflicto"),
            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor")
        };

        if (codigoEstado == HttpStatusCode.InternalServerError)
        {
            logger.LogError(excepcion, "Error no controlado al procesar {Metodo} {Ruta}", contexto.Request.Method, contexto.Request.Path);
        }

        var problema = new ProblemDetails
        {
            Status = (int)codigoEstado,
            Title = titulo,
            Detail = excepcion.Message,
            Instance = contexto.Request.Path
        };

        if (excepcion is ValidationException excepcionValidacion)
        {
            problema.Extensions["errores"] = excepcionValidacion.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        contexto.Response.ContentType = "application/problem+json";
        contexto.Response.StatusCode = (int)codigoEstado;
        await contexto.Response.WriteAsJsonAsync(problema);
    }
}
