using System.Reflection;
using GastosCompartidos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosCompartidos.Api.Data;

public class GastosCompartidosDbContext(DbContextOptions<GastosCompartidosDbContext> opciones) : DbContext(opciones)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Grupo> Grupos => Set<Grupo>();
    public DbSet<MiembroGrupo> MiembrosGrupo => Set<MiembroGrupo>();
    public DbSet<Gasto> Gastos => Set<Gasto>();
    public DbSet<ParticipacionGasto> ParticipacionesGasto => Set<ParticipacionGasto>();
    public DbSet<Liquidacion> Liquidaciones => Set<Liquidacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
