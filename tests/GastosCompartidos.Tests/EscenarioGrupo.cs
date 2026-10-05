using GastosCompartidos.Api.Data;
using GastosCompartidos.Api.Models;
using GastosCompartidos.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GastosCompartidos.Tests;

/// <summary>
/// Monta, directamente contra una base de datos SQLite en memoria (sin pasar por HTTP),
/// un grupo con N miembros para poder probar el cálculo de saldos y el algoritmo de
/// liquidación con escenarios numéricos exhaustivos de forma rápida y aislada.
/// </summary>
public sealed class EscenarioGrupo : IDisposable
{
    private readonly SqliteConnection _conexion;

    public GastosCompartidosDbContext Contexto { get; }
    public Grupo Grupo { get; }
    public List<Usuario> Usuarios { get; } = [];

    private EscenarioGrupo(SqliteConnection conexion, GastosCompartidosDbContext contexto, Grupo grupo)
    {
        _conexion = conexion;
        Contexto = contexto;
        Grupo = grupo;
    }

    public static async Task<EscenarioGrupo> CrearAsync(int numeroMiembros)
    {
        var conexion = new SqliteConnection("DataSource=:memory:");
        await conexion.OpenAsync();

        var contexto = new GastosCompartidosDbContext(
            new DbContextOptionsBuilder<GastosCompartidosDbContext>().UseSqlite(conexion).Options);
        await contexto.Database.EnsureCreatedAsync();

        var escenario = new EscenarioGrupo(conexion, contexto, new Grupo { Nombre = "Grupo de prueba", FechaCreacion = DateTime.UtcNow });

        for (var i = 0; i < numeroMiembros; i++)
        {
            var usuario = new Usuario { Nombre = $"Persona{i + 1}", Email = $"persona{i + 1}-{Guid.NewGuid()}@test.com", PasswordHash = "x" };
            contexto.Usuarios.Add(usuario);
            escenario.Usuarios.Add(usuario);
        }

        await contexto.SaveChangesAsync();

        foreach (var usuario in escenario.Usuarios)
        {
            escenario.Grupo.Miembros.Add(new MiembroGrupo { UsuarioId = usuario.Id, Rol = RolGrupo.Miembro });
        }

        contexto.Grupos.Add(escenario.Grupo);
        await contexto.SaveChangesAsync();

        return escenario;
    }

    public int IdDe(int indice) => Usuarios[indice].Id;

    public async Task AnadirGastoPartesIgualesAsync(int pagadorIndice, decimal importe, params int[] participantesIndices)
    {
        var participantes = participantesIndices.Select(i => Usuarios[i].Id).ToList();
        var reparto = RepartidorGastos.RepartirPartesIguales(importe, participantes);

        Contexto.Gastos.Add(new Gasto
        {
            GrupoId = Grupo.Id,
            PagadoPorId = Usuarios[pagadorIndice].Id,
            Importe = importe,
            Concepto = "Gasto de prueba",
            Fecha = DateTime.UtcNow,
            Participaciones = reparto.Select(r => new ParticipacionGasto { UsuarioId = r.UsuarioId, Importe = r.Importe }).ToList()
        });

        await Contexto.SaveChangesAsync();
    }

    public async Task AnadirGastoPersonalizadoAsync(int pagadorIndice, decimal importe, params (int Indice, decimal Importe)[] participaciones)
    {
        Contexto.Gastos.Add(new Gasto
        {
            GrupoId = Grupo.Id,
            PagadoPorId = Usuarios[pagadorIndice].Id,
            Importe = importe,
            Concepto = "Gasto de prueba",
            Fecha = DateTime.UtcNow,
            Participaciones = participaciones
                .Select(p => new ParticipacionGasto { UsuarioId = Usuarios[p.Indice].Id, Importe = p.Importe })
                .ToList()
        });

        await Contexto.SaveChangesAsync();
    }

    public async Task AnadirLiquidacionAsync(int deIndice, int paraIndice, decimal importe)
    {
        Contexto.Liquidaciones.Add(new Liquidacion
        {
            GrupoId = Grupo.Id,
            DeId = Usuarios[deIndice].Id,
            ParaId = Usuarios[paraIndice].Id,
            Importe = importe,
            Fecha = DateTime.UtcNow
        });

        await Contexto.SaveChangesAsync();
    }

    public ISaldoService CrearServicioSaldos() => new SaldoService(Contexto);

    public void Dispose()
    {
        Contexto.Dispose();
        _conexion.Dispose();
    }
}
