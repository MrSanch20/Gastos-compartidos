using GastosCompartidos.Api.Data;
using GastosCompartidos.Api.DTOs;
using GastosCompartidos.Api.DTOs.Gastos;
using GastosCompartidos.Api.Excepciones;
using GastosCompartidos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosCompartidos.Api.Services;

public class GastoService(GastosCompartidosDbContext contexto) : IGastoService
{
    public async Task<PaginaResultado<GastoDto>> ListarAsync(
        int grupoId, int usuarioId, int pagina, int tamanoPagina,
        DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct)
    {
        await VerificarMiembroAsync(grupoId, usuarioId, ct);

        var consulta = contexto.Gastos
            .Where(g => g.GrupoId == grupoId);

        if (fechaDesde is not null)
        {
            consulta = consulta.Where(g => g.Fecha >= fechaDesde.Value);
        }

        if (fechaHasta is not null)
        {
            consulta = consulta.Where(g => g.Fecha <= fechaHasta.Value);
        }

        var totalElementos = await consulta.CountAsync(ct);

        var gastos = await consulta
            .OrderByDescending(g => g.Fecha)
            .ThenByDescending(g => g.Id)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Include(g => g.PagadoPor)
            .Include(g => g.Participaciones).ThenInclude(p => p.Usuario)
            .ToListAsync(ct);

        var elementos = gastos.Select(MapearADto).ToList();

        return new PaginaResultado<GastoDto>(elementos, totalElementos, pagina, tamanoPagina);
    }

    public async Task<GastoDto> CrearAsync(int grupoId, CrearGastoRequest request, int usuarioId, CancellationToken ct)
    {
        var grupo = await VerificarMiembroAsync(grupoId, usuarioId, ct);

        VerificarQueSonMiembros(grupo, request.PagadoPorId, request.Participaciones.Select(p => p.UsuarioId));

        var participaciones = CalcularParticipaciones(request.Importe, request.TipoReparto, request.Participaciones);

        var gasto = new Gasto
        {
            GrupoId = grupoId,
            PagadoPorId = request.PagadoPorId,
            Importe = request.Importe,
            Concepto = request.Concepto,
            Fecha = request.Fecha,
            Participaciones = participaciones
        };

        contexto.Gastos.Add(gasto);
        await contexto.SaveChangesAsync(ct);

        return await ObtenerDtoAsync(gasto.Id, ct);
    }

    public async Task<GastoDto> EditarAsync(int grupoId, int gastoId, ActualizarGastoRequest request, int usuarioId, CancellationToken ct)
    {
        var grupo = await VerificarMiembroAsync(grupoId, usuarioId, ct);

        var gasto = await contexto.Gastos
            .Include(g => g.Participaciones)
            .SingleOrDefaultAsync(g => g.Id == gastoId && g.GrupoId == grupoId, ct)
            ?? throw new NoEncontradoException("El gasto no existe en este grupo.");

        VerificarQueSonMiembros(grupo, request.PagadoPorId, request.Participaciones.Select(p => p.UsuarioId));

        var participaciones = CalcularParticipaciones(request.Importe, request.TipoReparto, request.Participaciones);

        gasto.Concepto = request.Concepto;
        gasto.Importe = request.Importe;
        gasto.Fecha = request.Fecha;
        gasto.PagadoPorId = request.PagadoPorId;

        contexto.ParticipacionesGasto.RemoveRange(gasto.Participaciones);
        gasto.Participaciones = participaciones;

        await contexto.SaveChangesAsync(ct);

        return await ObtenerDtoAsync(gasto.Id, ct);
    }

    public async Task BorrarAsync(int grupoId, int gastoId, int usuarioId, CancellationToken ct)
    {
        await VerificarMiembroAsync(grupoId, usuarioId, ct);

        var gasto = await contexto.Gastos
            .SingleOrDefaultAsync(g => g.Id == gastoId && g.GrupoId == grupoId, ct)
            ?? throw new NoEncontradoException("El gasto no existe en este grupo.");

        contexto.Gastos.Remove(gasto);
        await contexto.SaveChangesAsync(ct);
    }

    private static List<ParticipacionGasto> CalcularParticipaciones(
        decimal importeTotal, TipoReparto tipoReparto, List<ParticipacionRequest> participacionesSolicitadas)
    {
        if (tipoReparto == TipoReparto.PartesIguales)
        {
            var usuarioIds = participacionesSolicitadas.Select(p => p.UsuarioId).ToList();
            return RepartidorGastos.RepartirPartesIguales(importeTotal, usuarioIds)
                .Select(r => new ParticipacionGasto { UsuarioId = r.UsuarioId, Importe = r.Importe })
                .ToList();
        }

        var sumaParticipaciones = participacionesSolicitadas.Sum(p => p.Importe!.Value);
        if (sumaParticipaciones != importeTotal)
        {
            throw new ReglaNegocioException(
                $"La suma de las participaciones ({sumaParticipaciones:0.00}) debe ser igual al importe total ({importeTotal:0.00}).");
        }

        return participacionesSolicitadas
            .Select(p => new ParticipacionGasto { UsuarioId = p.UsuarioId, Importe = p.Importe!.Value })
            .ToList();
    }

    private static void VerificarQueSonMiembros(Grupo grupo, int pagadoPorId, IEnumerable<int> participantesIds)
    {
        var idsMiembros = grupo.Miembros.Select(m => m.UsuarioId).ToHashSet();

        if (!idsMiembros.Contains(pagadoPorId))
        {
            throw new ReglaNegocioException("El usuario que pagó el gasto debe ser miembro del grupo.");
        }

        if (participantesIds.Any(id => !idsMiembros.Contains(id)))
        {
            throw new ReglaNegocioException("Todos los participantes del gasto deben ser miembros del grupo.");
        }
    }

    private async Task<Grupo> VerificarMiembroAsync(int grupoId, int usuarioId, CancellationToken ct)
    {
        var grupo = await contexto.Grupos
            .Include(g => g.Miembros)
            .SingleOrDefaultAsync(g => g.Id == grupoId, ct)
            ?? throw new NoEncontradoException("El grupo no existe.");

        if (!grupo.Miembros.Any(m => m.UsuarioId == usuarioId))
        {
            throw new PermisoDenegadoException("No eres miembro de este grupo.");
        }

        return grupo;
    }

    private async Task<GastoDto> ObtenerDtoAsync(int gastoId, CancellationToken ct)
    {
        var gasto = await contexto.Gastos
            .Include(g => g.PagadoPor)
            .Include(g => g.Participaciones).ThenInclude(p => p.Usuario)
            .SingleAsync(g => g.Id == gastoId, ct);

        return MapearADto(gasto);
    }

    private static GastoDto MapearADto(Gasto gasto) => new(
        gasto.Id,
        gasto.GrupoId,
        gasto.PagadoPorId,
        gasto.PagadoPor.Nombre,
        gasto.Importe,
        gasto.Concepto,
        gasto.Fecha,
        gasto.Participaciones
            .Select(p => new ParticipacionDto(p.UsuarioId, p.Usuario.Nombre, p.Importe))
            .ToList());
}
