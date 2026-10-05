using GastosCompartidos.Api.Data;
using GastosCompartidos.Api.DTOs.Saldos;
using GastosCompartidos.Api.Excepciones;
using GastosCompartidos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosCompartidos.Api.Services;

public class SaldoService(GastosCompartidosDbContext contexto) : ISaldoService
{
    public async Task<List<SaldoDto>> CalcularSaldosAsync(int grupoId, int usuarioId, CancellationToken ct)
    {
        var grupo = await CargarGrupoAsync(grupoId, ct);
        VerificarEsMiembro(grupo, usuarioId);
        return await CalcularSaldosCoreAsync(grupo, ct);
    }

    public async Task<List<PagoPropuestoDto>> ProponerLiquidacionAsync(int grupoId, int usuarioId, CancellationToken ct)
    {
        var grupo = await CargarGrupoAsync(grupoId, ct);
        VerificarEsMiembro(grupo, usuarioId);
        var saldos = await CalcularSaldosCoreAsync(grupo, ct);
        return CalculadoraLiquidacion.ProponerPagos(saldos);
    }

    public async Task<LiquidacionDto> RegistrarLiquidacionAsync(
        int grupoId, RegistrarLiquidacionRequest request, int usuarioId, CancellationToken ct)
    {
        var grupo = await CargarGrupoAsync(grupoId, ct);
        VerificarEsMiembro(grupo, usuarioId);

        var idsMiembros = grupo.Miembros.Select(m => m.UsuarioId).ToHashSet();
        if (!idsMiembros.Contains(request.DeId) || !idsMiembros.Contains(request.ParaId))
        {
            throw new ReglaNegocioException("Tanto el pagador como el receptor deben ser miembros del grupo.");
        }

        var liquidacion = new Liquidacion
        {
            GrupoId = grupoId,
            DeId = request.DeId,
            ParaId = request.ParaId,
            Importe = request.Importe,
            Fecha = request.Fecha ?? DateTime.UtcNow
        };

        contexto.Liquidaciones.Add(liquidacion);
        await contexto.SaveChangesAsync(ct);

        var de = grupo.Miembros.Single(m => m.UsuarioId == request.DeId).Usuario;
        var para = grupo.Miembros.Single(m => m.UsuarioId == request.ParaId).Usuario;

        return new LiquidacionDto(liquidacion.Id, grupoId, de.Id, de.Nombre, para.Id, para.Nombre, liquidacion.Importe, liquidacion.Fecha);
    }

    public async Task<decimal> ObtenerSaldoAsync(int grupoId, int usuarioId, CancellationToken ct)
    {
        var grupo = await CargarGrupoAsync(grupoId, ct);
        var saldos = await CalcularSaldosCoreAsync(grupo, ct);
        return saldos.SingleOrDefault(s => s.UsuarioId == usuarioId)?.Saldo ?? 0m;
    }

    /// <summary>
    /// Calcula el saldo de cada miembro actual del grupo:
    /// lo que ha pagado, menos lo que le corresponde de cada gasto, ajustado por las
    /// liquidaciones ya registradas (quien paga una liquidación reduce su deuda;
    /// quien la recibe reduce lo que se le debe).
    /// </summary>
    private async Task<List<SaldoDto>> CalcularSaldosCoreAsync(Grupo grupo, CancellationToken ct)
    {
        var gastos = await contexto.Gastos
            .Where(g => g.GrupoId == grupo.Id)
            .Include(g => g.Participaciones)
            .ToListAsync(ct);

        var liquidaciones = await contexto.Liquidaciones
            .Where(l => l.GrupoId == grupo.Id)
            .ToListAsync(ct);

        var saldos = new Dictionary<int, decimal>();

        foreach (var gasto in gastos)
        {
            saldos[gasto.PagadoPorId] = saldos.GetValueOrDefault(gasto.PagadoPorId) + gasto.Importe;

            foreach (var participacion in gasto.Participaciones)
            {
                saldos[participacion.UsuarioId] = saldos.GetValueOrDefault(participacion.UsuarioId) - participacion.Importe;
            }
        }

        foreach (var liquidacion in liquidaciones)
        {
            saldos[liquidacion.DeId] = saldos.GetValueOrDefault(liquidacion.DeId) + liquidacion.Importe;
            saldos[liquidacion.ParaId] = saldos.GetValueOrDefault(liquidacion.ParaId) - liquidacion.Importe;
        }

        return grupo.Miembros
            .Select(m => new SaldoDto(m.UsuarioId, m.Usuario.Nombre, Math.Round(saldos.GetValueOrDefault(m.UsuarioId), 2)))
            .ToList();
    }

    private async Task<Grupo> CargarGrupoAsync(int grupoId, CancellationToken ct)
    {
        return await contexto.Grupos
            .Include(g => g.Miembros).ThenInclude(m => m.Usuario)
            .SingleOrDefaultAsync(g => g.Id == grupoId, ct)
            ?? throw new NoEncontradoException("El grupo no existe.");
    }

    private static void VerificarEsMiembro(Grupo grupo, int usuarioId)
    {
        if (!grupo.Miembros.Any(m => m.UsuarioId == usuarioId))
        {
            throw new PermisoDenegadoException("No eres miembro de este grupo.");
        }
    }
}
