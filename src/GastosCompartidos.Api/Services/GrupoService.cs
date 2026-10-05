using GastosCompartidos.Api.Data;
using GastosCompartidos.Api.DTOs.Grupos;
using GastosCompartidos.Api.Excepciones;
using GastosCompartidos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosCompartidos.Api.Services;

public class GrupoService(GastosCompartidosDbContext contexto) : IGrupoService
{
    public async Task<List<GrupoResumenDto>> ListarMisGruposAsync(int usuarioId, CancellationToken ct)
    {
        return await contexto.MiembrosGrupo
            .Where(m => m.UsuarioId == usuarioId)
            .Select(m => new GrupoResumenDto(m.Grupo.Id, m.Grupo.Nombre, m.Grupo.FechaCreacion))
            .ToListAsync(ct);
    }

    public async Task<GrupoDetalleDto> ObtenerDetalleAsync(int grupoId, int usuarioId, CancellationToken ct)
    {
        var grupo = await contexto.Grupos
            .Include(g => g.Miembros).ThenInclude(m => m.Usuario)
            .SingleOrDefaultAsync(g => g.Id == grupoId, ct)
            ?? throw new NoEncontradoException("El grupo no existe.");

        if (!grupo.Miembros.Any(m => m.UsuarioId == usuarioId))
        {
            throw new PermisoDenegadoException("No eres miembro de este grupo.");
        }

        var miembros = grupo.Miembros
            .Select(m => new MiembroDto(m.UsuarioId, m.Usuario.Nombre, m.Usuario.Email, m.Rol))
            .ToList();

        return new GrupoDetalleDto(grupo.Id, grupo.Nombre, grupo.FechaCreacion, miembros);
    }

    public async Task<GrupoResumenDto> CrearAsync(CrearGrupoRequest request, int usuarioCreadorId, CancellationToken ct)
    {
        var grupo = new Grupo
        {
            Nombre = request.Nombre,
            FechaCreacion = DateTime.UtcNow,
            Miembros =
            {
                new MiembroGrupo { UsuarioId = usuarioCreadorId, Rol = RolGrupo.Admin }
            }
        };

        contexto.Grupos.Add(grupo);
        await contexto.SaveChangesAsync(ct);

        return new GrupoResumenDto(grupo.Id, grupo.Nombre, grupo.FechaCreacion);
    }

    public async Task AnadirMiembroAsync(int grupoId, AnadirMiembroRequest request, int usuarioSolicitanteId, CancellationToken ct)
    {
        var grupo = await contexto.Grupos
            .Include(g => g.Miembros)
            .SingleOrDefaultAsync(g => g.Id == grupoId, ct)
            ?? throw new NoEncontradoException("El grupo no existe.");

        var miembroSolicitante = grupo.Miembros.SingleOrDefault(m => m.UsuarioId == usuarioSolicitanteId)
            ?? throw new PermisoDenegadoException("No eres miembro de este grupo.");

        if (miembroSolicitante.Rol != RolGrupo.Admin)
        {
            throw new PermisoDenegadoException("Solo el administrador del grupo puede añadir miembros.");
        }

        var usuarioNuevo = await contexto.Usuarios.SingleOrDefaultAsync(u => u.Email == request.Email, ct)
            ?? throw new NoEncontradoException("No existe ningún usuario registrado con ese email.");

        if (grupo.Miembros.Any(m => m.UsuarioId == usuarioNuevo.Id))
        {
            throw new ConflictoException("Ese usuario ya es miembro del grupo.");
        }

        grupo.Miembros.Add(new MiembroGrupo { GrupoId = grupoId, UsuarioId = usuarioNuevo.Id, Rol = RolGrupo.Miembro });
        await contexto.SaveChangesAsync(ct);
    }

    public async Task QuitarMiembroAsync(int grupoId, int usuarioAQuitarId, int usuarioSolicitanteId, CancellationToken ct)
    {
        var grupo = await contexto.Grupos
            .Include(g => g.Miembros)
            .SingleOrDefaultAsync(g => g.Id == grupoId, ct)
            ?? throw new NoEncontradoException("El grupo no existe.");

        var miembroSolicitante = grupo.Miembros.SingleOrDefault(m => m.UsuarioId == usuarioSolicitanteId)
            ?? throw new PermisoDenegadoException("No eres miembro de este grupo.");

        if (miembroSolicitante.Rol != RolGrupo.Admin)
        {
            throw new PermisoDenegadoException("Solo el administrador del grupo puede quitar miembros.");
        }

        var miembroAQuitar = grupo.Miembros.SingleOrDefault(m => m.UsuarioId == usuarioAQuitarId)
            ?? throw new NoEncontradoException("Ese usuario no es miembro del grupo.");

        contexto.MiembrosGrupo.Remove(miembroAQuitar);
        await contexto.SaveChangesAsync(ct);
    }
}
