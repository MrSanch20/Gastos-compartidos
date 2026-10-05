using GastosCompartidos.Api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GastosCompartidos.Tests;

/// <summary>
/// Factory de pruebas de integración: sustituye la base de datos real por SQLite en memoria
/// y fija la configuración de JWT, para que los tests no dependan de user-secrets ni de
/// ningún estado externo (también funcionan igual en un pipeline de CI).
/// </summary>
public class GastosCompartidosWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _conexion = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _conexion.Open();

        builder.ConfigureAppConfiguration((_, configuracion) =>
        {
            configuracion.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Clave"] = "ClaveDePruebasDeIntegracion-NoUsarEnProduccion-1234567890",
                ["Jwt:Emisor"] = "GastosCompartidosApi.Tests",
                ["Jwt:Audiencia"] = "GastosCompartidosClientes.Tests",
                ["Jwt:MinutosExpiracion"] = "120"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<GastosCompartidosDbContext>>();

            services.AddDbContext<GastosCompartidosDbContext>(opciones => opciones.UseSqlite(_conexion));

            using var scope = services.BuildServiceProvider().CreateScope();
            var contexto = scope.ServiceProvider.GetRequiredService<GastosCompartidosDbContext>();
            contexto.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _conexion.Dispose();
        }
    }
}
